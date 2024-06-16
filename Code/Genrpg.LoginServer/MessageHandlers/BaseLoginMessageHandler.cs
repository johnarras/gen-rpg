using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Servers.LoginServer;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.MessageHandlers
{
    public abstract class BaseLoginMessageHandler<T> : ILoginMessageHandler where T : ILoginQueueMessage
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
