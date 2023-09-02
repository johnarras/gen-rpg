using Genrpg.PlayerServer.Services;
using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Messages;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public abstract class BasePlayerMessageHandler<T> : IPlayerMessageHandler where T : IPlayerCloudMessage
    {

        protected IPlayerService _playerService;

        protected abstract Task InnerHandleMessage(ServerGameState gs, T message);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task HandleMessage(ServerGameState gs, ICloudMessage message, CancellationToken token)
        {
            await InnerHandleMessage(gs, (T)message);
        }
    }
}
