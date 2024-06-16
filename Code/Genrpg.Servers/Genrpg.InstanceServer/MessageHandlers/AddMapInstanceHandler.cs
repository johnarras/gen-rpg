using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Logging.Interfaces;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class AddMapInstanceHandler : BaseInstanceMessageHandler<AddMapInstance>
    {
        protected override async Task InnerHandleMessage(AddMapInstance message)
        {
            _logService.Message("Received " + message.GetType().Name + " from " + message.ServerId);
            await _instanceManagerService.AddInstanceData(message);
            await Task.CompletedTask;
        }
    }
}
