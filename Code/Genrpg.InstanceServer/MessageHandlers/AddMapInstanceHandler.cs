﻿using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.Core;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class AddMapInstanceHandler : BaseInstanceMessageHandler<AddMapInstance>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, AddMapInstance message)
        {
            gs.logger.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.AddInstanceData(message);
            await Task.CompletedTask;
        }
    }
}
