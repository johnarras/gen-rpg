using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
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
            _logService.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.AddMapServer(message);
            await Task.CompletedTask;
        }
    }
}
