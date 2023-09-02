using Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Messaging;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class RemoveMapServerHandler : BaseInstanceMessageHandler<RemoveMapServer>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, RemoveMapServer message)
        {
            gs.logger.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _mapInstanceService.RemoveMapServer(message);
            await Task.CompletedTask;
        }
    }
}
