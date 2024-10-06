#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using System;
using System.Collections.Generic;

using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using System.Threading;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities.TCP;
using Genrpg.Shared.Networking.MapApiSerializers;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Genrpg.Shared.Logging.Interfaces;
using UnityEngine;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Tokens;

public interface IRealtimeNetworkService : IInitializable, IMapTokenService
{
    void CloseClient();
    void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer);
    ConnMessageCounts GetMessageCounts();
    void SendMapMessage(IPlayerCommand message);

}


public class RealtimeNetworkService : IRealtimeNetworkService
{
    private ILogService _logService;
    protected IClientGameState _gs = null;
    protected IMapGenData _md;
    public RealtimeNetworkService(CancellationToken token)
    {
        TaskUtils.ForgetAwaitable(ProcessMessages(token));
    }


    CancellationTokenSource _mapTokenSource = null;
    private CancellationToken _token;
    public void SetMapToken(CancellationToken token)
    {
        _mapTokenSource?.Cancel();
        _mapTokenSource?.Dispose();
        _mapTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        _token = _mapTokenSource.Token;
    }

    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }


    public CancellationToken GetToken()
    {
        return _token;
    }
   
    public void CloseClient()
    {
        if (_clientConn != null)
        {
            _clientConn.Shutdown(null,"CloseClient");
                
            _clientConn = null;
        }
    }

    private SetupDictionaryContainer<Type, IClientMapMessageHandler> _mapMessageHandlers = new SetupDictionaryContainer<Type, IClientMapMessageHandler>();
    private string _host = "";
    private long _port = 0;
    private EMapApiSerializers _serializer = EMapApiSerializers.Json;

    public void SetRealtimeEndpoint(string host, long port, EMapApiSerializers serializer)
    {
        _host = host;
        _port = port;
        _serializer = serializer;
    }

    protected ConcurrentQueue<IMapApiMessage> _messages = new ConcurrentQueue<IMapApiMessage>();
    protected void HandleMapMessages(List<IMapApiMessage> results, CancellationToken token, object optionalData)
    {
        foreach (IMapApiMessage message in results)
        {
            _messages.Enqueue(message);
        }
    }

    protected async Awaitable ProcessMessages(CancellationToken token)
    {
        while (true)
        {
            await Awaitable.NextFrameAsync(cancellationToken: token);
            while (_messages.TryDequeue(out IMapApiMessage message))
            {                
                HandleOneMapApiMessage(message, token);
            }
        }
    }

    public void SendMapMessage(IPlayerCommand message)
    {
        SendMessageList(new List<IPlayerCommand>() { message });
    }

    public void SendMessages(List<IPlayerCommand> messages)
    {
        SendMessageList(messages);
    }

    protected void SendMessageList (List<IPlayerCommand> messages)
    {

#if SHOW_SEND_RECEIVE_MESSAGES
        StringBuilder sb = new StringBuilder();
        sb.Append("Commands: ");
        foreach (IMessage message in messages)
        {
            sb.Append(message.GetType().Name + " -- ");
        }
        _logService.Debug(sb.ToString());
#endif
            
        string userid = "";
        string sessionid = "";

        if (_md.GeneratingMap)
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
            _logService.Info("Create Realtime Client: " + _host + " " + _port);
            _clientConn = new ConnectTcpConn(_host, _port, MapApiSerializerFactory.Create(_serializer),
                HandleMapMessages,
                _logService, _token, null);

        }

        foreach (IPlayerCommand message in messages)
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
                _logService.Error("Null result found");
                return;
            }
            Type resType = message.GetType();

            if (!_mapMessageHandlers.TryGetValue(resType, out IClientMapMessageHandler handler))
            {
                _logService.Error("Missing Typed Result Handler for: " + resType);
                return;
            }

            handler.Process(message as IMapApiMessage, token);
        }
        catch (Exception ee)
        {
            _logService.Exception(ee, "HandleOneMapAPIMessage");
        }
    }

}
