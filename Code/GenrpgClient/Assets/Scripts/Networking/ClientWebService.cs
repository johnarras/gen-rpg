#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Assets.Scripts.Tokens;
using System.Threading;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.Shared.Website.Messages;
using Assets.Scripts.Login.Messages;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using System.Net;
using static ClientWebService;
using System.Runtime.InteropServices;
using Genrpg.Shared.HelperClasses;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using Assets.Scripts.Login.MessageHandlers;

public delegate void WebResultsHandler(string txt, CancellationToken token);

public interface IClientWebService : IInitializable, IGameTokenService
{
    void SendAuthWebCommand(IAuthCommand loginCommand, CancellationToken token);
    void SendClientWebCommand(IWebCommand data, CancellationToken token);
    void SendNoUserWebCommand(INoUserCommand data, CancellationToken token);
    void HandleResults(string txt, CancellationToken token);
}


public class ClientWebService : IClientWebService
{
    private Dictionary<string,WebRequestQueue> _queues = new Dictionary<string,WebRequestQueue>();

    private SetupDictionaryContainer<Type, IClientLoginResultHandler> _loginResultHandlers = new SetupDictionaryContainer<Type, IClientLoginResultHandler>();

    protected IServiceLocator _loc = null;
    protected IUnityGameState _gs = null;
    private IUnityUpdateService _updateService;
    protected ILogService _logService;
    public ClientWebService(CancellationToken token)
    {
        _token = token;
    }

    // Web endpoints.
    public const string UserEndpoint = "/client";
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
        _queues[UserEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + UserEndpoint, UserRequestDelaySeconds, _logService, this, _queues[AuthEndpoint]);
        _queues[NoUserEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + NoUserEndpoint, 0, _logService, this, null);

        foreach (var queue in _queues.Values)
        {
            _loc.Resolve(queue);
        }


        _updateService.AddUpdate(this, ProcessRequestQueues, UpdateType.Late);


        await Task.CompletedTask;
    }
    private class FullLoginCommand
    {
        public IWebCommand Command;
        public CancellationToken Token;
    }

    private class ResultHandlerPair
    {
        public IWebResult Result { get; set; } = null;
        public IClientLoginResultHandler Handler { get; set; } = null;
    }

    public void HandleResults(string txt, CancellationToken token)
    {
        try
        {
            LoginServerResultSet resultSet = SerializationUtils.Deserialize<LoginServerResultSet>(txt);

            List<ResultHandlerPair> resultPairs = new List<ResultHandlerPair>();

            foreach (IWebResult result in resultSet.Results)
            {
                if (_loginResultHandlers.TryGetValue(result.GetType(), out IClientLoginResultHandler handler))
                {
                    resultPairs.Add(new ResultHandlerPair()
                    {
                        Result = result,
                        Handler = handler,
                    });
                }
                else
                {
                    _logService.Error("Unknown Message From Login Server: " + result.GetType().Name);
                }
            }

            resultPairs = resultPairs.OrderByDescending(x => x.Handler.Priority()).ToList();

            foreach (ResultHandlerPair resultPair in resultPairs)
            {
                resultPair.Handler.Process(resultPair.Result, token);
            }
        }
        catch (Exception ex)
        {
            _logService.Exception(ex, "ProcessWebResponses");
        }


    }


    private class WebRequestQueue
    {
        private List<FullLoginCommand> _queue = new List<FullLoginCommand>();
        private List<FullLoginCommand> _pending = new List<FullLoginCommand>();
        private float _delaySeconds;
        private CancellationToken _token;
        private IUnityGameState _gs;
        private DateTime _lastResponseReceivedTime = DateTime.UtcNow;
        private WebRequestQueue _parentQueue;
        private List<WebRequestQueue> _childQueues = new List<WebRequestQueue>();
        private string _fullEndpoint;
        private ILogService _logService;
        private IClientWebService _clientWebService;

        public WebRequestQueue(IUnityGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, ILogService logService, IClientWebService _clientWebService, WebRequestQueue parentQueue = null)
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

        public void AddRequest(IWebCommand command, CancellationToken token)
        {
            _queue.Add(new FullLoginCommand(){ Command = command, Token = token });
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

            _pending = new List<FullLoginCommand>(_queue);
            _queue.Clear();

            ClientWebRequest req = new ClientWebRequest();

            WebServerCommandSet commandSet = new WebServerCommandSet()
            {
                UserId = _gs?.user?.Id ?? null,
                SessionId = _gs?.user?.SessionId ?? null,
            };

            List<CancellationToken> allTokens = _pending.Select(x => x.Token).Distinct().ToList();
            allTokens.Add(_token);

            CancellationTokenSource fullRequestSource = CancellationTokenSource.CreateLinkedTokenSource(allTokens.ToArray());

            commandSet.Commands.AddRange(_pending.Select(x => x.Command));

            string commandText = SerializationUtils.Serialize(commandSet);

            AwaitableUtils.ForgetAwaitable(req.SendRequest(_logService, _fullEndpoint, commandText, HandleResults, fullRequestSource.Token));
        }

        public void HandleResults(string txt, CancellationToken token)
        {
            _clientWebService.HandleResults(txt, token);
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

    public void SendAuthWebCommand(IAuthCommand authCommand, CancellationToken token)
    {
        SendRequest(AuthEndpoint, authCommand, token);
    }
    public void SendClientWebCommand(IWebCommand userCommand, CancellationToken token)
    {
        SendRequest(UserEndpoint, userCommand, token);
    }

    public void SendNoUserWebCommand(INoUserCommand noUserCommand, CancellationToken token)
    {
        SendRequest(NoUserEndpoint, noUserCommand, token);
    }

    private void SendRequest(string endpoint, IWebCommand loginCommand, CancellationToken token)
    {
        if (_queues.TryGetValue(endpoint, out WebRequestQueue queue))
        {
            queue.AddRequest(loginCommand, token);
        }
    }
}
