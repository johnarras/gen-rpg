using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
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
            _logService.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.RemoveMapServer(message);
            await Task.CompletedTask;
        }
    }
}
