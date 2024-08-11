using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Networking.Entities;
using Genrpg.Shared.Networking.Entities.TCP;
using Genrpg.Shared.Networking.Interfaces;
using Genrpg.Shared.Networking.MapApiSerializers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.MapServer.Networking.Listeners
{


    public class BaseTcpListener : IListener
    {
        protected TcpListener _server = null;
        protected List<BaseTcpConn> _connections = new List<BaseTcpConn>();

        protected object _connLock = new object();

        protected string _host = null;
        protected int _port = 0;
        protected CancellationToken _token;
        protected EMapApiSerializers _seralizer;
        ILogService _logger = null;
        protected Action<ServerConnectionState> _addConnectionHandler;
        protected MapApiMessageHandler _messageHandler;

        public virtual void Dispose()
        {
            _server.Stop();
            foreach (IConnection conn in _connections)
            {
                conn.ForceClose();
            }
        }

        public BaseTcpListener (string host, int port,
            ILogService logger,
            EMapApiSerializers serializer,
            Action<ServerConnectionState> addConnection, 
            MapApiMessageHandler receiveMessages,
            CancellationToken token)
        {
            _logger = logger;
            _addConnectionHandler = addConnection;
            _messageHandler = receiveMessages;
            _port = port;
            _host = host;
            _token = token;
            _seralizer = serializer;
            Task.Run(() => RunListener(), _token);
        }

        private async Task AddClient(TcpClient client)
        {
            ServerConnectionState connState = new ServerConnectionState();
            IConnection conn = CreateTCPConnection(client, connState, _logger, _seralizer);
            connState.conn = conn;
            _addConnectionHandler(connState);
            await Task.CompletedTask;
        }

        protected IConnection CreateTCPConnection(TcpClient client, ServerConnectionState connState, ILogService logger, EMapApiSerializers serializer)
        {
            return new AcceptTcpConn(client, MapApiSerializerFactory.Create(serializer),
                _messageHandler,
            logger,
            _token,
            connState);
        }

        private async Task RunListener()
        {
            bool listenerIsActive = false;
            try
            {
                while (true)
                {
                    if (!listenerIsActive)
                    {
                        _logger.Info("Create listen socket " + _host + " " + _port);
                        IPAddress localAddr = IPAddress.Parse(_host);
                        _server = new TcpListener(localAddr, _port);
                        _server.Start();
                        listenerIsActive = true;
                    }

                    TcpClient client = await _server.AcceptTcpClientAsync(_token);
                    _logger.Info("Accepted client on " + _host + " " + _port);
                    _ = Task.Run(() => AddClient(client));
                }
            }
            catch (SocketException e)
            {
                Trace.WriteLine("SocketException: {0}", e.Message);
                _server.Stop();
                listenerIsActive = false;
            }
            catch (OperationCanceledException ce)
            {
                _logger.Info("Shutdown listen socket " + ce.Message);
                _server.Stop();
            }
            catch (Exception e)
            {
                _logger.Exception(e, "BaseTcpListener.Listen");
            }
        }
    }
}

