using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Servers.WebServer;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.MessageHandlers
{
    public abstract class BaseWebMessageHandler<T> : IWebMessageHandler where T : ILoginQueueMessage
    {

        protected abstract Task InnerHandleMessage(T message);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task HandleMessage(IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage((T)message);
        }
    }
}
