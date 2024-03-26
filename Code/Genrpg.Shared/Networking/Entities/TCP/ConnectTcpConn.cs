using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Networking.MapApiSerializers;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.Shared.Networking.Entities.TCP
{
    public class ConnectTcpConn : BaseTcpConn
    {
        const int MaxConnectTries = 3;
        string _host;
        int _port;

        public ConnectTcpConn(string host, long port,
            IMapApiSerializer serializer,
            MapApiMessageHandler handler, 
            ILogService logger,
            CancellationToken token, object extraData) : base(serializer, handler,logger, token, extraData)
        {
            _host = host;
            _port = (int)port;

            Task.Run(() => ConnectToServer());
        }

        protected async Task ConnectToServer()
        {
            TcpClient client = new TcpClient();
            for (int times = 0; times < MaxConnectTries; times++)
            {
                try
                {
                    using (Task connectTask = client.ConnectAsync(_host, _port))
                    {
                        connectTask.Wait(2000);

                        if (connectTask.IsCompleted && !connectTask.IsCanceled)
                        {
                            InitTcpClient(client);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Shutdown(e, "TcpClient could not connect " + _host + ": " + _port);
                }
            }
            await Task.CompletedTask;
        }
    }
}
