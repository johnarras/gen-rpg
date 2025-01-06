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

public delegate void WebResultsHandler(string txt, List<FullWebCommand> commands, CancellationToken token);


public enum EWebCommandState
{
    Pending,
    Complete,
}

public class FullWebCommand
{
    public IWebCommand Command;
    public CancellationToken Token;
    public Type ResultType { get; set; }
    public object ResultObject { get; set; }
    public ErrorResult ErrorResult { get; set; }
    public EWebCommandState State { get; set; } = EWebCommandState.Pending;
}

public interface IClientWebService : IInitializable, IGameTokenService
{
    void SendAuthWebCommand(IAuthCommand loginCommand, CancellationToken token);
    Awaitable<T> SendAuthWebCommandAsync<T>(IAuthCommand userCommand, CancellationToken token);

    void SendClientWebCommand(IWebCommand data, CancellationToken token);
    Awaitable<T> SendClientWebCommandAsync<T>(IWebCommand userCommand, CancellationToken token);

    void SendNoUserWebCommand(INoUserCommand data, CancellationToken token);
    Awaitable<T> SendNoUserWebCommandAsync<T>(INoUserCommand userCommand, CancellationToken token);

    void HandleResults(string txt, List<FullWebCommand> commands, CancellationToken token);
}


public class ClientWebService : IClientWebService
{
    private class ResultHandlerPair
    {
        public IWebResult Result { get; set; } = null;
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

    public void HandleResults(string txt, List<FullWebCommand> commands, CancellationToken token)
    {
        try
        {
            LoginServerResultSet resultSet = SerializationUtils.Deserialize<LoginServerResultSet>(txt);

            List<ResultHandlerPair> resultPairs = new List<ResultHandlerPair>();

            foreach (IWebResult result in resultSet.Results)
            {
                bool foundAsyncCommand = false;
                if (commands != null)
                {
                    FullWebCommand command = commands.FirstOrDefault(x => x.ResultType == result.GetType());
                    if (command != null)
                    {
                        command.ResultObject = result;
                        foundAsyncCommand = true;
                    }
                }
                if (_loginResultHandlers.TryGetValue(result.GetType(), out IClientLoginResultHandler handler))
                {
                    resultPairs.Add(new ResultHandlerPair()
                    {
                        Result = result,
                        Handler = handler,
                    });
                }
                else if (!foundAsyncCommand)
                {
                    _logService.Error("Unknown Message From Login Server: " + result.GetType().Name);
                }
            }

            if (commands != null)
            {
                foreach (FullWebCommand fullWebCommand in commands)
                {
                    fullWebCommand.State = EWebCommandState.Complete;
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
        private List<FullWebCommand> _queue = new List<FullWebCommand>();
        private List<FullWebCommand> _pending = new List<FullWebCommand>();
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

        public FullWebCommand AddRequest(IWebCommand command, CancellationToken token, Type resultType = null)
        {
            FullWebCommand fullWebCommand = new FullWebCommand() { Command = command, Token = token, ResultType = resultType };
            _queue.Add(fullWebCommand);
            return fullWebCommand;
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

            _pending = new List<FullWebCommand>(_queue);
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

            _awaitableService.ForgetAwaitable(req.SendRequest(_logService, _fullEndpoint, commandText, _pending.ToList(), HandleResults, fullRequestSource.Token));
        }

        public void HandleResults(string txt, List<FullWebCommand> commands, CancellationToken token)
        {
            _clientWebService.HandleResults(txt, commands, token);
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

    public async Awaitable<T> SendAuthWebCommandAsync<T>(IAuthCommand userCommand, CancellationToken token)
    {
        return await SendWebCommandAsync<T>(AuthEndpoint, userCommand, token);
    }



    public void SendClientWebCommand(IWebCommand userCommand, CancellationToken token)
    {
        SendRequest(ClientEndpoint, userCommand, token);
    }
    
    public async Awaitable<T> SendClientWebCommandAsync<T>(IWebCommand userCommand, CancellationToken token)
    {
        return await SendWebCommandAsync<T>(ClientEndpoint, userCommand, token);
    }


    public void SendNoUserWebCommand(INoUserCommand noUserCommand, CancellationToken token)
    {
        SendRequest(NoUserEndpoint, noUserCommand, token);
    }

    public async Awaitable<T> SendNoUserWebCommandAsync<T>(INoUserCommand userCommand, CancellationToken token)
    {
        return await SendWebCommandAsync<T>(NoUserEndpoint, userCommand, token);
    }

    private FullWebCommand SendRequest(string endpoint, IWebCommand loginCommand, CancellationToken token, Type resultType = null)
    {
        if (_queues.TryGetValue(endpoint, out WebRequestQueue queue))
        {
           return queue.AddRequest(loginCommand, token, resultType);
        }
        return null;
    }

    private async Awaitable<T> SendWebCommandAsync<T>(string endpoint, IWebCommand webCommand, CancellationToken token)
    {
        FullWebCommand fullCommand = SendRequest(endpoint, webCommand, token, typeof(T));

        while (fullCommand.State == EWebCommandState.Pending)
        {
            await Awaitable.NextFrameAsync(token);
        }

        return (T)fullCommand.ResultObject;
    }


}
