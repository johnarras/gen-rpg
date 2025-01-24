#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using System.Threading;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Assets.Scripts.Login.Messages;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.HelperClasses;
using UnityEngine;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Tokens;
using Assets.Scripts.Awaitables;

public delegate void WebResultsHandler(string txt, List<FullWebRequest> requests, CancellationToken token);


public enum EWebRequestState
{
    Pending,
    Complete,
}

public class FullWebRequest
{
    public IWebRequest Request;
    public CancellationToken Token;
    public Type ResponseType { get; set; }
    public object ResponseObject { get; set; }
    public ErrorResponse ErrorResponse { get; set; }
    public EWebRequestState State { get; set; } = EWebRequestState.Pending;
}

public interface IClientWebService : IInitializable, IGameTokenService
{
    void SendAuthWebRequest(IAuthRequest loginRequest, CancellationToken token);
    Awaitable<T> SendAuthWebRequestAsync<T>(IAuthRequest userRequest, CancellationToken token);

    void SendClientUserWebRequest(IClientUserRequest data, CancellationToken token);
    Awaitable<T> SendClientUserWebRequestAsync<T>(IClientUserRequest userRequest, CancellationToken token);

    void SendNoUserWebRequest(INoUserRequest data, CancellationToken token);
    Awaitable<T> SendNoUserWebRequestAsync<T>(INoUserRequest userRequest, CancellationToken token);

    void HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token);
}


public class ClientWebService : IClientWebService
{
    private class ResultHandlerPair
    {
        public IWebResponse Result { get; set; } = null;
        public IClientLoginResultHandler Handler { get; set; } = null;
    }



    private Dictionary<string,WebRequestQueue> _queues = new Dictionary<string,WebRequestQueue>();

    private SetupDictionaryContainer<Type, IClientLoginResultHandler> _loginResultHandlers = new SetupDictionaryContainer<Type, IClientLoginResultHandler>();

    protected IServiceLocator _loc = null;
    protected IClientGameState _gs = null;
    private IClientUpdateService _updateService;
    protected ILogService _logService;
    private IAwaitableService _awaitableService;
    public ClientWebService()
    {
    }

    // Web endpoints.
    public const string ClientEndpoint = "/client";
    public const string AuthEndpoint = "/auth";
    public const string NoUserEndpoint = "/nouser";

    CancellationTokenSource _webTokenSource = null;
    private CancellationToken _token;
    public void SetGameToken(CancellationToken token)
    {
        _webTokenSource?.Cancel();
        _webTokenSource?.Dispose();
        _webTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _webTokenSource.Token;
    }

    private const float UserRequestDelaySeconds = 0.3f;

    public async Task Initialize(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_gs.LoginServerURL))
        {
            return;
        }
        // This is like this rather than a REST api because in games you want to be able to mix features, so gameplay is one concern and you
        // do not separate it. So your server should be a monolith, and I want to be able to batch requests.
        _queues[AuthEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + AuthEndpoint, UserRequestDelaySeconds, _logService, this, null);
        _queues[ClientEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + ClientEndpoint, UserRequestDelaySeconds, _logService, this, _queues[AuthEndpoint]);
        _queues[NoUserEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + NoUserEndpoint, 0, _logService, this, null);

        foreach (var queue in _queues.Values)
        {
            _loc.Resolve(queue);
        }


        _updateService.AddUpdate(this, ProcessRequestQueues, UpdateType.Late, token);


        await Task.CompletedTask;
    }

    public void HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token)
    {
        try
        {
            WebServerResponseSet responseSet = SerializationUtils.Deserialize<WebServerResponseSet>(txt);

            List<ResultHandlerPair> responsePairs = new List<ResultHandlerPair>();

            foreach (IWebResponse response in responseSet.Responses)
            {
                bool foundAsyncRequest = false;
                if (requests != null)
                {
                    FullWebRequest request = requests.FirstOrDefault(x => x.ResponseType == response.GetType());
                    if (request != null)
                    {
                        request.ResponseObject = response;
                        foundAsyncRequest = true;
                    }
                }
                if (_loginResultHandlers.TryGetValue(response.GetType(), out IClientLoginResultHandler handler))
                {
                    responsePairs.Add(new ResultHandlerPair()
                    {
                        Result = response,
                        Handler = handler,
                    });
                }
                else if (!foundAsyncRequest)
                {
                    _logService.Error("Unknown Message From Login Server: " + response.GetType().Name);
                }
            }

            if (requests != null)
            {
                foreach (FullWebRequest fullWebRequest in requests)
                {
                    fullWebRequest.State = EWebRequestState.Complete;
                }
            }

            responsePairs = responsePairs.OrderByDescending(x => x.Handler.Priority()).ToList();

            foreach (ResultHandlerPair responsePair in responsePairs)
            {
                responsePair.Handler.Process(responsePair.Result, token);
            }
        }
        catch (Exception ex)
        {
            _logService.Exception(ex, "ProcessWebResponses");
        }


    }


    private class WebRequestQueue
    {
        private List<FullWebRequest> _queue = new List<FullWebRequest>();
        private List<FullWebRequest> _pending = new List<FullWebRequest>();
        private float _delaySeconds;
        private CancellationToken _token;
        private IClientGameState _gs;
        private DateTime _lastResponseReceivedTime = DateTime.UtcNow;
        private WebRequestQueue _parentQueue;
        private List<WebRequestQueue> _childQueues = new List<WebRequestQueue>();
        private string _fullEndpoint;
        private ILogService _logService;
        private IClientWebService _clientWebService;
        private IAwaitableService _awaitableService;

        public WebRequestQueue(IClientGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, ILogService logService, IClientWebService _clientWebService, WebRequestQueue parentQueue = null)
        {
            _gs = gs;
            _parentQueue = parentQueue;
            _logService = logService;
            this._clientWebService = _clientWebService;
            if (_parentQueue != null)
            {
                _parentQueue.AddChildQueue(this);
            }
            _delaySeconds = delaySeconds;
            _token = token;         
            _fullEndpoint = fullEndpoint;

        }

        public void AddChildQueue(WebRequestQueue childQueue)
        {
            _childQueues.Add(childQueue);
        }

        public FullWebRequest AddRequest(IWebRequest request, CancellationToken token, Type responseType = null)
        {
            FullWebRequest fullWebRequest = new FullWebRequest() { Request = request, Token = token, ResponseType = responseType };
            _queue.Add(fullWebRequest);
            return fullWebRequest;
        }

        public bool HavePendingRequests()
        {
            return _pending.Count > 0 || (DateTime.UtcNow - _lastResponseReceivedTime).TotalSeconds < _delaySeconds;
        }

        public bool HaveRequests()
        {
            return _queue.Count > 0 || HavePendingRequests();
        }

        public void ProcessRequests()
        {
            if (_parentQueue != null && _parentQueue.HaveRequests())
            {
                return;
            }

            foreach (WebRequestQueue childQueue in _childQueues)
            {
                if (childQueue.HavePendingRequests())
                {
                    return;
                }
            }

            if (HavePendingRequests() || _queue.Count < 1)
            {
                return;
            }

            _pending = new List<FullWebRequest>(_queue);
            _queue.Clear();

            ClientWebRequest req = new ClientWebRequest();

            WebServerRequestSet requestSet = new WebServerRequestSet()
            {
                UserId = _gs?.user?.Id ?? null,
                SessionId = _gs?.user?.SessionId ?? null,
            };

            List<CancellationToken> allTokens = _pending.Select(x => x.Token).Distinct().ToList();
            allTokens.Add(_token);

            CancellationTokenSource fullRequestSource = CancellationTokenSource.CreateLinkedTokenSource(allTokens.ToArray());

            requestSet.Requests.AddRange(_pending.Select(x => x.Request));

            string requestText = SerializationUtils.Serialize(requestSet);

            _awaitableService.ForgetAwaitable(req.SendRequest(_logService, _fullEndpoint, requestText, _pending.ToList(), HandleResults, fullRequestSource.Token));
        }

        public void HandleResults(string txt, List<FullWebRequest> requests, CancellationToken token)
        {
            _clientWebService.HandleResponses(txt, requests, token);
            _lastResponseReceivedTime = DateTime.UtcNow;
            _pending.Clear();
        }
    }


    private void ProcessRequestQueues()
    {
        foreach (WebRequestQueue queue in _queues.Values)
        {
            queue.ProcessRequests();
        }
    }

    public CancellationToken GetToken()
    {
        return _token;
    }

    public void SendAuthWebRequest(IAuthRequest authRequest, CancellationToken token)
    {
        SendRequest(AuthEndpoint, authRequest, token);
    }

    public async Awaitable<T> SendAuthWebRequestAsync<T>(IAuthRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(AuthEndpoint, userRequest, token);
    }



    public void SendClientUserWebRequest(IClientUserRequest userRequest, CancellationToken token)
    {
        SendRequest(ClientEndpoint, userRequest, token);
    }
    
    public async Awaitable<T> SendClientUserWebRequestAsync<T>(IClientUserRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(ClientEndpoint, userRequest, token);
    }



    public void SendNoUserWebRequest(INoUserRequest noUserRequest, CancellationToken token)
    {
        SendRequest(NoUserEndpoint, noUserRequest, token);
    }

    public async Awaitable<T> SendNoUserWebRequestAsync<T>(INoUserRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(NoUserEndpoint, userRequest, token);
    }

    private FullWebRequest SendRequest(string endpoint, IWebRequest loginRequest, CancellationToken token, Type responseType = null)
    {
        if (_queues.TryGetValue(endpoint, out WebRequestQueue queue))
        {
           return queue.AddRequest(loginRequest, token, responseType);
        }
        return null;
    }

    private async Awaitable<T> SendWebRequestAsync<T>(string endpoint, IWebRequest webRequest, CancellationToken token)
    {
        FullWebRequest fullRequest = SendRequest(endpoint, webRequest, token, typeof(T));

        while (fullRequest.State == EWebRequestState.Pending)
        {
            await Awaitable.NextFrameAsync(token);
        }

        return (T)fullRequest.ResponseObject;
    }


}
