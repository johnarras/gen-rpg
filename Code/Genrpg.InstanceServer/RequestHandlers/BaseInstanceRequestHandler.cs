
using Genrpg.InstanceServer.RequestHandlers;
using Genrpg.InstanceServer.Services;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.InstanceServer.Requests;
using Genrpg.ServerShared.Core;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public abstract class BaseInstanceRequestHandler<T> : IInstanceRequestHandler where T : IInstanceServerRequest
    {
        protected IMapInstanceService _mapInstanceService = null;

        protected abstract Task<IResponse> InnerHandleRequest(ServerGameState gs, T request, ResponseEnvelope envelope);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task<IResponse> HandleRequest(ServerGameState gs, IRequest request, ResponseEnvelope envelope, CancellationToken token)
        {
            return await InnerHandleRequest(gs, (T)request, envelope);
        }
    }
}
