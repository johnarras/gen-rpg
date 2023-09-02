using Genrpg.PlayerServer.RequestHandlers;
using Genrpg.PlayerServer.Services;
using Genrpg.ServerShared.CloudMessaging.Messages;
using Genrpg.ServerShared.CloudMessaging.Requests;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Messages;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.Core;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public abstract class BasePlayerRequestHandler<T> : IPlayerRequestHandler where T : IPlayerCloudRequest
    {

        protected IPlayerService _playerService;

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
