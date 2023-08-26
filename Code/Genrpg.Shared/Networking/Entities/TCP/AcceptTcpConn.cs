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
            Action<List<IMapApiMessage>,CancellationToken> messageHandler, 
            ILogSystem logger, 
            CancellationToken token) : base(serializer, messageHandler, logger, token)
        {
            InitTcpClient(client);
        }
    }
}
