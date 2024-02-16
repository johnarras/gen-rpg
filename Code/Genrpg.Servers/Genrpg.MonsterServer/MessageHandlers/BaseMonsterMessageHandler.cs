using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MonsterServer.MessageHandlers
{
    public abstract class BaseMonsterMessageHandler<T> : IMonsterMessageHandler where T : IQueueMessage
    {
        protected abstract Task InnerHandleMessage(ServerGameState gs, T message);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task HandleMessage(ServerGameState gs, IQueueMessage message, CancellationToken token)
        {
            await InnerHandleMessage(gs, (T)message);
        }
    }
}
