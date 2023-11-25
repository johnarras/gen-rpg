using Genrpg.PlayerServer.Managers;
using Genrpg.PlayerServer.RequestHandlers;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Requests;
using Genrpg.ServerShared.Core;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public abstract class BasePlayerRequestHandler<T> : IPlayerRequestHandler where T : IPlayerServerRequest
    {

        protected IPlayerService _playerService;

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
