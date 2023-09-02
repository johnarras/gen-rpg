
using Genrpg.InstanceServer.RequestHandlers;
using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.ServerShared.CloudMessaging.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.InstanceServer.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.MapInstance.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Messages;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.Core;

namespace Genrpg.InstanceServer.MessageHandlers
{
    public abstract class BaseInstanceRequestHandler<T> : IInstanceRequestHandler where T : IInstanceCloudRequest
    {

        protected abstract Task<ICloudResponse> InnerHandleRequest(ServerGameState gs, T request);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task<ICloudResponse> HandleRequest(ServerGameState gs, ICloudRequest request, CancellationToken token)
        {
            return await InnerHandleRequest(gs, (T)request);
        }
    }
}
