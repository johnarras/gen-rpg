using Genrpg.Shared.Logs.Entities;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Networking.MapApiSerializers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Genrpg.Shared.Networking.Entities.TCP
{
    public class AcceptTcpConn : BaseTcpConn
    {

        public AcceptTcpConn(TcpClient client, IMapApiSerializer serializer,  
            MapApiMessageHandler messageHandler, 
            ILogSystem logger, 
            CancellationToken token, ServerConnectionState connState) : base(serializer, messageHandler, logger, token, connState)
        {
            InitTcpClient(client);
        }
    }
}
