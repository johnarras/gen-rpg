using Genrpg.ServerShared.CloudMessaging.Messages.InstanceServer;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class AddMapServerHandler : BaseInstanceMessageHandler<AddMapServer>
    {
        protected override async Task InnerHandleMessage(ServerGameState gs, AddMapServer message)
        {
            gs.logger.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _mapInstanceService.AddMapServer(message);
            await Task.CompletedTask;
        }
    }
}
