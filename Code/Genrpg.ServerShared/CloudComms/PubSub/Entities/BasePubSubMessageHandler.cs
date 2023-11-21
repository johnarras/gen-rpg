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

        public async Task HandleMessage(ServerGameState gs, IPubSubMessage message, CancellationToken token)
        {
            await InnerHandleMessage(gs,(M)message,token);
        }

        protected abstract Task InnerHandleMessage(ServerGameState gs, M message, CancellationToken token);
    }
}
