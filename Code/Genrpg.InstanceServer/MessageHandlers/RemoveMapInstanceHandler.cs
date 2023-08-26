using Genrpg.ServerShared.CloudMessaging.Messages.InstanceServer;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class RemoveMapInstanceHandler : BaseInstanceMessageHandler<RemoveMapInstance>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, RemoveMapInstance message)
        {
            gs.logger.Message("Received " + message.GetType().Name + " from " + message.MapInstanceId);
            await _mapInstanceService.RemoveInstanceData(message);
            await Task.CompletedTask;
        }
    }
}
