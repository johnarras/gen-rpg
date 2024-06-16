using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.CloudComms.PubSub.Entities
{
    public abstract class BasePubSubMessageHandler<M> : IPubSubMessageHandler where M : class, IPubSubMessage
    {
        public abstract Type GetKey();

        public async Task HandleMessage(IPubSubMessage message, CancellationToken token)
        {
            await InnerHandleMessage((M)message,token);
        }

        protected abstract Task InnerHandleMessage(M message, CancellationToken token);
    }
}
