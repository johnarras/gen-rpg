#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;

using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Assets.Scripts.Tokens;
using System.Threading;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages;
using Assets.Scripts.Login.Messages;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using System.Net;
using static WebNetworkService;
using System.Runtime.InteropServices;
using Genrpg.Shared.HelperClasses;
using System.Text.RegularExpressions;

public delegate void WebResultsHandler(string txt, CancellationToken token);

public interface IWebNetworkService : IInitializable, IGameTokenService
{
    void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token);
    void SendClientWebCommand(IClientCommand data, CancellationToken token);
    void SendNoUserWebCommand(INoUserCommand data, CancellationToken token);
}

public class WebNetworkService : IWebNetworkService
{
    private Dictionary<string,WebRequestQueue> _queues = new Dictionary<string,WebRequestQueue>();

    private class FullLoginCommand
    {
        public ILoginCommand Command;
        public CancellationToken Token;
    }

    private class ResultHandlerPair
    {
        public ILoginResult Result { get; set; } = null;
        public IClientLoginResultHandler Handler { get; set; } = null;
    }

    private class WebRequestQueue
    {
        private SetupDictionaryContainer<Type, IClientLoginResultHandler> _loginResultHandlers = new SetupDictionaryContainer<Type, IClientLoginResultHandler>();
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
        private IServiceLocator _loc;

        public WebRequestQueue(IUnityGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, ILogService logService, WebRequestQueue parentQueue = null)
        {
            _gs = gs;
            _parentQueue = parentQueue;
            _logService = logService;
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

        public void AddRequest(ILoginCommand command, CancellationToken token)
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

            LoginServerCommandSet commandSet = new LoginServerCommandSet()
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

        private void HandleResults(string txt, CancellationToken token)
        {

            try
            {
                LoginServerResultSet resultSet = SerializationUtils.Deserialize<LoginServerResultSet>(txt);

                List<ResultHandlerPair> resultPairs = new List<ResultHandlerPair>();

                foreach (ILoginResult result in resultSet.Results)
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
            catch(Exception ex)
            {
                _logService.Exception(ex, "ProcessWebResponses");
            }
                
        
            _lastResponseReceivedTime = DateTime.UtcNow;
            _pending.Clear();
        }
    }

    protected IServiceLocator _loc = null;
    protected IUnityGameState _gs = null;
    private IUnityUpdateService _updateService;
    private ILogService _logService;
    public WebNetworkService(CancellationToken token)
    {
    }

    // Web endpoints.
    public const string UserEndpoint = "/client";
    public const string LoginEndpoint = "/login";
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
        // This is like this rather than a REST api because in games you want to be able to mix features, so gameplay is one concern and you
        // do not separate it. So your server should be a monolith, and I want to be able to batch requests.
        _queues[LoginEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + LoginEndpoint, UserRequestDelaySeconds, _logService, null);
        _queues[UserEndpoint] = new WebRequestQueue(_gs,token, _gs.LoginServerURL + UserEndpoint, UserRequestDelaySeconds, _logService, _queues[LoginEndpoint]);
        _queues[NoUserEndpoint] = new WebRequestQueue(_gs, token, _gs.LoginServerURL + NoUserEndpoint, 0, _logService, null);

        foreach (var queue in _queues.Values)
        {
            _loc.Resolve(queue);
        }


        _updateService.AddUpdate(this, ProcessRequestQueues, UpdateType.Late);


        await Task.CompletedTask;
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

    public void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token)
    {
        SendRequest(LoginEndpoint, loginCommand, token);
    }
    public void SendClientWebCommand(IClientCommand userCommand, CancellationToken token)
    {
        SendRequest(UserEndpoint, userCommand, token);
    }

    public void SendNoUserWebCommand(INoUserCommand noUserCommand, CancellationToken token)
    {
        SendRequest(NoUserEndpoint, noUserCommand, token);
    }

    private void SendRequest(string endpoint, ILoginCommand loginCommand, CancellationToken token)
    {
        if (_queues.TryGetValue(endpoint, out WebRequestQueue queue))
        {
            queue.AddRequest(loginCommand, token);
        }
    }
}
