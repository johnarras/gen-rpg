using Genrpg.InstanceServer.Entities;
using Genrpg.InstanceServer.MessageHandlers;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Requests;
using Genrpg.ServerShared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.InstanceServer.RequestHandlers
{
    public class GetInstanceDataHandler : BaseInstanceRequestHandler<GetInstanceRequest>
    {
        protected override async Task<IResponse> InnerHandleRequest(ServerGameState gs, GetInstanceRequest request, ResponseEnvelope envelope)
        {
            MapInstanceData instanceData = await _mapInstanceService.GetInstanceDataForMap(request.MapId);

            if (instanceData == null)
            {
                envelope.ErrorText = "Missing Map Instance";
                return null;
            }
            else
            {
                GetInstanceResponse response = new GetInstanceResponse()
                {
                    Host = instanceData.Host,
                    Port = instanceData.Port,
                    InstanceId = instanceData.InstanceId,
                };
                return response;
            }
        }
    }
}
