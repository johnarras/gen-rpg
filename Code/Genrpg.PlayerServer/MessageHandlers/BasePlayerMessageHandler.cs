﻿using Genrpg.PlayerServer.Managers;
using Genrpg.ServerShared.CloudComms.Queues.Entities;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public abstract class BasePlayerMessageHandler<T> : IPlayerMessageHandler where T : IPlayerQueueMessage
    {

        protected IPlayerService _playerService;

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
