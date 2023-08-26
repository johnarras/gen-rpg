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
using System.Threading.Tasks;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Login.Messages;
using Assets.Scripts.Login.Messages;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities.TCP;
using Genrpg.Shared.Networking.MapApiSerializers;

public delegate void WebResultsHandler(UnityGameState gs, string txt, CancellationToken token);


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
    private IReflectionService _reflectionService;
    public NetworkService(UnityGameState gs)
    {
        _gs = gs;
    }

    // Web endpoints.
    public const string ClientEndpoint = "/Client";
    public const string LoginEndpoint = "/Login";

    private CancellationToken _token;
    public void SetToken(CancellationToken token)
    {
        _token = token;
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
        InnerSendWebRequest(LoginEndpoint, loginCommand, token);
    }

    public void SendClientWebCommand(ILoginCommand data, CancellationToken token)
    {
        InnerSendWebRequest(ClientEndpoint, data, token);
    }

    public void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer)
    {
        _host = host;
        _port = port;
        _serializer = serializer;
    }

    protected void InnerSendWebRequest(string endpoint, ILoginCommand data,
        CancellationToken token)
    {
        WebRequest req = new WebRequest();

        LoginServerCommandSet commandSet = new LoginServerCommandSet()
        {
            UserId = _gs?.user?.Id ?? null,
            SessionId = _gs?.user?.SessionId ?? null,
        };
        commandSet.Commands.Add(data);

        string commandText = SerializationUtils.Serialize(commandSet);

        req.GetData(_gs, _webURI + endpoint, commandText, HandleLoginResults, token).Forget();  
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
    }

    public async Task Setup(GameState gs, CancellationToken token)
    {
        if (gs is UnityGameState ugs)
        {
            _mapMessageHandlers = _reflectionService.SetupDictionary<Type, IClientMapMessageHandler>(gs);
            _loginResultHandlers = _reflectionService.SetupDictionary<Type, IClientLoginResultHandler>(gs);
            _webURI = ugs.SiteURL;
        }
        await Task.CompletedTask;
    }

    protected async UniTask HandleMapMessages(List<IMapApiMessage> results, CancellationToken token)
    {
        if (!TokenUtils.IsValid(token))
        {
            return;
        }
        await UniTask.NextFrame(token);
        object exceptionObj = null;
        try
        {
#if SHOW_SEND_RECEIVE_MESSAGES
            StringBuilder sb = new StringBuilder();
            sb.Append("Results: ");
            foreach (IMessage message in results)
            {
                sb.Append(message.GetType().Name + " -- ");
            }

            _gs.logger.Debug(sb.ToString());
#endif
            if (!TokenUtils.IsValid(token))
            {
                return;
            }
            foreach (object res in results)
            {
                exceptionObj = res;
                HandleOneResult(res, token);
            }
        }
        catch (Exception e)
        {
            _gs.logger.Exception(e, "RealtimeHandleOneResult"); 
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
            _clientConn = new ConnectTcpConn(_host, _port, MapApiSerializerFactory.Create(_serializer), (List<IMapApiMessage> objs, CancellationToken token) => { HandleMapMessages(objs, _token).Forget(); }, _gs.logger, _token);

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

    private void HandleOneResult(object resObj, CancellationToken token)
    {
        if (resObj == null)
        {
            _gs.logger.Error("Null result found");
            return;
        }
        Type resType = resObj.GetType();

        if (!_mapMessageHandlers.ContainsKey(resType))
        {
            _gs.logger.Error("Missing Typed Result Handler for: " + resType);
            return;
        }

        _mapMessageHandlers[resType].Process(_gs, resObj as IMapApiMessage, token);
    }
}
