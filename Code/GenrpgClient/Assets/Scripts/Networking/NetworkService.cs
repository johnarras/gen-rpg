#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public delegate void WebResultsHandler(UnityGameState gs, string txt, CancellationToken token);


internal class FullLoginCommand
{
    ILoginCommand Command;

}

public interface INetworkService : ISetupService, IMapTokenService
{
    string GetBaseURI();
    void CloseClient();
    void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token);
    void SendClientWebCommand(ILoginCommand data, CancellationToken token);
    void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer);
    ConnMessageCounts GetMessageCounts();
    void SendMapMessage(IMapApiMessage message);

}

public class NetworkService : INetworkService
{
    protected UnityGameState _gs = null;
    private IReflectionService _reflectionService = null!;
    private IUnityUpdateService _updateService = null!;
    public NetworkService(UnityGameState gs, CancellationToken token)
    {
        _gs = gs;
        TaskUtils.AddTask(ProcessMessages(),"processmessages", token);
    }

    // Web endpoints.
    public const string ClientEndpoint = "/Client";
    public const string LoginEndpoint = "/Login";

    DateTime _lastLoginResultsReceived = DateTime.UtcNow;

    CancellationTokenSource _networkTokenSource = null;
    private CancellationToken _token;
    public void SetToken(CancellationToken token)
    {
        _networkTokenSource?.Cancel();
        _networkTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _networkTokenSource.Token;
    }

    public async Task Setup(GameState gs, CancellationToken token)
    {
        if (gs is UnityGameState ugs)
        {
            _mapMessageHandlers = _reflectionService.SetupDictionary<Type, IClientMapMessageHandler>(gs);
            _loginResultHandlers = _reflectionService.SetupDictionary<Type, IClientLoginResultHandler>(gs);
            _webURI = ugs.SiteURL;
        }
        _updateService.AddTokenUpdate(this, ProcessLoginMessages, UpdateType.Late);
        await Task.CompletedTask;
    }


    private List<ILoginCommand> _clientCommandQueue = new List<ILoginCommand>();
    private List<ILoginCommand> _currentClientCommands = new List<ILoginCommand>();
    private void ProcessLoginMessages(CancellationToken token)
    {
        if (_currentClientCommands.Count > 0 || _clientCommandQueue.Count < 1 ||
            (DateTime.UtcNow-_lastLoginResultsReceived).TotalSeconds < 0.3f)
        {
            return;
        }

        _currentClientCommands = _clientCommandQueue.ToList();
        _clientCommandQueue.Clear();

        InnerSendWebRequest(ClientEndpoint, _currentClientCommands, token);
    }

    public CancellationToken GetToken()
    {
        return _token;
    }


    public string GetBaseURI()
    {
        return _webURI;
    }
            
    public void CloseClient()
    {
        if (_clientConn != null)
        {
            _clientConn.Shutdown(null,"CloseClient");
                
            _clientConn = null;
        }
    }

    private Dictionary<Type,IClientMapMessageHandler> _mapMessageHandlers = null;
    private Dictionary<Type, IClientLoginResultHandler> _loginResultHandlers = null;
    private string _webURI = null;
    private string _host = "";
    private long _port = 0;
    private EMapApiSerializers _serializer = EMapApiSerializers.Json;

    public void SendLoginWebCommand(LoginCommand loginCommand, CancellationToken token)
    {
        _currentClientCommands.Add(loginCommand);
        InnerSendWebRequest(LoginEndpoint, new List<ILoginCommand>() { loginCommand }, token);
    }

    public void SendClientWebCommand(ILoginCommand command, CancellationToken token)
    {
        _clientCommandQueue.Add(command);
    }

    public void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer)
    {
        _host = host;
        _port = port;
        _serializer = serializer;
    }

    protected void InnerSendWebRequest(string endpoint, List<ILoginCommand> commands,
        CancellationToken token)
    {
        ClientWebRequest req = new ClientWebRequest();

        LoginServerCommandSet commandSet = new LoginServerCommandSet()
        {
            UserId = _gs?.user?.Id ?? null,
            SessionId = _gs?.user?.SessionId ?? null,
        };
        commandSet.Commands = commands;

        string commandText = SerializationUtils.Serialize(commandSet);

        TaskUtils.AddTask(req.SendRequest(_gs, _webURI + endpoint, commandText, HandleLoginResults, token),"sendrequest",token);  
    }

    private void HandleLoginResults (UnityGameState gs, string txt, CancellationToken token)
    {
        LoginServerResultSet resultSet = SerializationUtils.Deserialize<LoginServerResultSet>(txt);

        foreach (ILoginResult result in resultSet.Results)
        {
            if (_loginResultHandlers.TryGetValue(result.GetType(), out IClientLoginResultHandler handler))
            {
                handler.Process(gs, result, token);
            }
            else
            {
                _gs.logger.Error("Unknown Message From Login Server: " + result.GetType().Name);
            }
        }
        _lastLoginResultsReceived = DateTime.UtcNow;
        _currentClientCommands.Clear();
    }

    protected ConcurrentQueue<IMapApiMessage> _messages = new ConcurrentQueue<IMapApiMessage>();
    protected void HandleMapMessages(List<IMapApiMessage> results, CancellationToken token, object optionalData)
    {
        foreach (IMapApiMessage message in results)
        {
            _messages.Enqueue(message);
        }
    }

    protected async Task ProcessMessages()
    {
        while (true)
        {
            await Task.Delay(1).ConfigureAwait(true);
            while (_messages.TryDequeue(out IMapApiMessage message))
            {                
                HandleOneMapApiMessage(message, _token);
            }
        }
    }

    public void SendMapMessage(IMapApiMessage message)
    {
        SendMessageList(new List<IMapApiMessage>() { message });
    }

    public void SendMessages(List<IMapApiMessage> messages)
    {
        SendMessageList(messages);
    }

    protected void SendMessageList (List<IMapApiMessage> messages)
    {

#if SHOW_SEND_RECEIVE_MESSAGES
        StringBuilder sb = new StringBuilder();
        sb.Append("Commands: ");
        foreach (IMessage message in messages)
        {
            sb.Append(message.GetType().Name + " -- ");
        }
        _gs.logger.Debug(sb.ToString());
#endif
            
        string userid = "";
        string sessionid = "";

        if (_gs.md == null || _gs.md.GeneratingMap)
        {
            return;
        }

        if (_gs.user != null)
        {
            userid = _gs.user.Id;
            sessionid = _gs.user.SessionId;
        }

        if (_clientConn == null || _clientConn.RemoveMe())
        {
            _clientConn = new ConnectTcpConn(_host, _port, MapApiSerializerFactory.Create(_serializer),
                HandleMapMessages,
                _gs.logger, _token, null);

        }

        foreach (IMapApiMessage message in messages)
        {
            _clientConn.AddMessage(message);
        }
    }

    public ConnMessageCounts GetMessageCounts()
    {
        if (_clientConn != null && !_clientConn.RemoveMe())
        {
            return _clientConn.GetCounts();
        }

        return new ConnMessageCounts();
    }

    private IConnection _clientConn = null;

    private void HandleOneMapApiMessage(IMapApiMessage message, CancellationToken token)
    {
        try
        {
            if (message == null)
            {
                _gs.logger.Error("Null result found");
                return;
            }
            Type resType = message.GetType();

            if (!_mapMessageHandlers.ContainsKey(resType))
            {
                _gs.logger.Error("Missing Typed Result Handler for: " + resType);
                return;
            }

            _mapMessageHandlers[resType].Process(_gs, message as IMapApiMessage, token);
        }
        catch (Exception ee)
        {
            _gs.logger.Exception(ee, "HandleOneMapAPIMessage");
        }
    }

}
