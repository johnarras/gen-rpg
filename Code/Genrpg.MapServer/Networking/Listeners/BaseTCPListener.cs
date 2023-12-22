using Genrpg.Shared.Logs.Interfaces;
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
        ILogSystem _logger = null;
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
            EMapApiSerializers serializer,
            Action<ServerConnectionState> addConnection, 
            MapApiMessageHandler receiveMessages,
            CancellationToken token)
        {
            _addConnectionHandler = addConnection;
            _messageHandler = receiveMessages;
            _port = port;
            _host = host;
            _token = token;
            _seralizer = serializer;
            Task.Run(() => RunListener(), _token);
        }

        private void AddClient(TcpClient client)
        {
            ServerConnectionState connState = new ServerConnectionState();
            IConnection conn = CreateTCPConnection(client, connState, _logger, _seralizer);
            connState.conn = conn;
            _addConnectionHandler(connState);
        }

        protected IConnection CreateTCPConnection(TcpClient client, ServerConnectionState connState, ILogSystem logger, EMapApiSerializers serializer)
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
            while (true)
            {
                try
                {
                    if (!listenerIsActive)
                    {
                        IPAddress localAddr = IPAddress.Parse(_host);
                        _server = new TcpListener(localAddr, _port);
                        _server.Start();
                        listenerIsActive = true;
                    }

                    TcpClient client = await _server.AcceptTcpClientAsync(_token);
                    AddClient(client);
                }
                catch (SocketException e)
                {
                    Trace.WriteLine("SocketException: {0}", e.Message);
                    _server.Stop();
                    listenerIsActive = false;
                }
            }
        }
    }
}

