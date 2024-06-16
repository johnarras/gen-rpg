using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
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

        protected override async Task InnerHandleMessage(RemoveMapInstance message)
        {
            _logService.Message("Received " + message.GetType().Name + " from " + message.FullInstanceId);
            await _instanceManagerService.RemoveInstanceData(message);
            await Task.CompletedTask;
        }
    }
}
