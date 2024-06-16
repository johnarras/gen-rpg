using Azure.Core;
using Genrpg.InstanceServer.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Queues;
using Genrpg.ServerShared.CloudComms.Servers.LoginServer;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Core;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public class GetInstanceQueueRequestHandler : BaseInstanceMessageHandler<GetInstanceQueueRequest>
    {

        private ICloudCommsService _commsService = null;

        protected override async Task InnerHandleMessage(GetInstanceQueueRequest message)
        {
            MapInstanceData instanceData = await _instanceManagerService.GetInstanceDataForMap(message.MapId);
            _logService.Info("Instance Data for mapId: " + message.MapId + " is " + instanceData);
            GetInstanceQueueResponse response = new GetInstanceQueueResponse()
            {
                RequestId = message.RequestId,
            };

            if (instanceData == null)
            {
                response.ErrorText = "Missing Map Instance " + message.MapId;
            }
            else
            {
                response.Host = instanceData.Host;
                response.Port = instanceData.Port;
                response.InstanceId = instanceData.InstanceId;
            }
            _commsService.SendQueueMessage(message.FromServerId, response);
        }
    }
}
