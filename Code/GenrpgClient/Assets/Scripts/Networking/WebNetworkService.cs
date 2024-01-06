#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Reflection.Services;
using Assets.Scripts.Tokens;
using System.Threading;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages;
using Assets.Scripts.Login.Messages;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities.TCP;
using Genrpg.Shared.Networking.MapApiSerializers;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

public delegate void WebResultsHandler(UnityGameState gs, string txt, CancellationToken token);

public interface IWebNetworkService : ISetupService, IGameTokenService
{
    string GetBaseURI();
    void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token);
    void SendClientWebCommand(ILoginCommand data, CancellationToken token);

}


public class WebNetworkService : IWebNetworkService
{

    private List<FullLoginCommand> _clientCommandQueue = new List<FullLoginCommand>();
    private List<FullLoginCommand> _currentClientCommands = new List<FullLoginCommand>();


    internal class FullLoginCommand
    {
        public ILoginCommand Command;
        public CancellationToken Token;
    }


    internal class ResultHandlerPair
    {
        public ILoginResult Result { get; set; } = null;
        public IClientLoginResultHandler Handler { get; set; } = null;
    }

    protected UnityGameState _gs = null;
    private IReflectionService _reflectionService = null!;
    private IUnityUpdateService _updateService = null!;
    public WebNetworkService(UnityGameState gs, CancellationToken token)
    {
        _gs = gs;
    }

    // Web endpoints.
    public const string ClientEndpoint = "/Client";
    public const string LoginEndpoint = "/Login";

    DateTime _lastLoginResultsReceived = DateTime.UtcNow;

    CancellationTokenSource _webTokenSource = null;
    private CancellationToken _token;
    public void SetGameToken(CancellationToken token)
    {
        _webTokenSource?.Cancel();
        _webTokenSource?.Dispose();        
        _webTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _webTokenSource.Token;
    }

    public async Task Setup(GameState gs, CancellationToken token)
    {
        if (gs is UnityGameState ugs)
        {
            _loginResultHandlers = _reflectionService.SetupDictionary<Type, IClientLoginResultHandler>(gs);
            _webURI = ugs.SiteURL;
        }
        _updateService.AddUpdate(this, ProcessLoginMessages, UpdateType.Late);
        await UniTask.CompletedTask;
    }

    private void ProcessLoginMessages()
    {
        if (_currentClientCommands.Count > 0 || _clientCommandQueue.Count < 1 ||
            (DateTime.UtcNow - _lastLoginResultsReceived).TotalSeconds < 0.3f)
        {
            return;
        }

        _currentClientCommands = _clientCommandQueue.ToList();
        _clientCommandQueue.Clear();

        InnerSendWebRequest(ClientEndpoint, _currentClientCommands);
    }

    public CancellationToken GetToken()
    {
        return _token;
    }


    public string GetBaseURI()
    {
        return _webURI;
    }

    private Dictionary<Type, IClientLoginResultHandler> _loginResultHandlers = null;
    private string _webURI = null;
    private string _host = "";
    private long _port = 0;
    private EMapApiSerializers _serializer = EMapApiSerializers.Json;

    public void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token)
    {
        _currentClientCommands.Add(new FullLoginCommand() { Command = loginCommand, Token = token});
        InnerSendWebRequest(LoginEndpoint, _currentClientCommands);
    }

    public void SendClientWebCommand(ILoginCommand command, CancellationToken token)
    {
        _clientCommandQueue.Add(new FullLoginCommand() { Command = command, Token = token });
    }

    public void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer)
    {
        _host = host;
        _port = port;
        _serializer = serializer;
    }

    private void InnerSendWebRequest(string endpoint, List<FullLoginCommand> commands)
    {
        ClientWebRequest req = new ClientWebRequest();

        LoginServerCommandSet commandSet = new LoginServerCommandSet()
        {
            UserId = _gs?.user?.Id ?? null,
            SessionId = _gs?.user?.SessionId ?? null,
        };

        List<CancellationToken> allTokens = commands.Select(x => x.Token).Distinct().ToList();
        allTokens.Add(_token);

        CancellationTokenSource fullRequestSource = CancellationTokenSource.CreateLinkedTokenSource(allTokens.ToArray());

        commandSet.Commands.AddRange(commands.Select(x => x.Command));

        string commandText = SerializationUtils.Serialize(commandSet);

        req.SendRequest(_gs, _webURI + endpoint, commandText, HandleLoginResults, fullRequestSource.Token).Forget();
    }

    private void HandleLoginResults(UnityGameState gs, string txt, CancellationToken token)
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
                _gs.logger.Error("Unknown Message From Login Server: " + result.GetType().Name);
            }
        }

        resultPairs = resultPairs.OrderByDescending(x => x.Handler.Priority()).ToList();

        foreach (ResultHandlerPair resultPair in resultPairs)
        {
            resultPair.Handler.Process(gs, resultPair.Result, token);
        }

        _lastLoginResultsReceived = DateTime.UtcNow;
        _currentClientCommands.Clear();
    }
}
