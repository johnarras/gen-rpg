using Genrpg.PlayerServer.RequestHandlers;
using Genrpg.ServerShared.CloudComms.Requests.Entities;
using Genrpg.ServerShared.CloudComms.Servers.MapInstance.Requests;
using Genrpg.ServerShared.Core;

namespace Genrpg.PlayerServer.MessageHandlers
{
    public abstract class BaseMonsterRequestHandler<T> : IMonsterRequestHandler where T : IMonsterServerRequest
    {

        protected abstract Task<IResponse> InnerHandleRequest(ServerGameState gs, T request);

        public Type GetKey()
        {
            return typeof(T);
        }

        public async Task<IResponse> HandleRequest(ServerGameState gs, IRequest request, ResponseEnvelope envelope, CancellationToken token)
        {
            return await InnerHandleRequest(gs, (T)request);
        }
    }
}
