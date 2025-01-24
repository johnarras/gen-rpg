using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;

namespace Genrpg.RequestServer.ClientUser.RequestHandlers
{
    public abstract class BaseClientUserRequestHandler<TRequest> : IClientUserRequestHandler where TRequest : IClientUserRequest
    {

        protected IPlayerDataService _playerDataService = null;
        protected ILoginPlayerDataService _loginPlayerDataService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IServerConfig _config = null;
        protected IWebServerService _loginServerService = null;

        protected abstract Task InnerHandleMessage(WebContext context, TRequest request, CancellationToken token);

        public Type GetKey()
        {
            return typeof(TRequest);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IWebRequest request, CancellationToken token)
        {
            await InnerHandleMessage(context, (TRequest)request, token);
        }

        protected void ShowError(WebContext context, string msg)
        {
            context.Responses.Add(new ErrorResponse() { Error = msg });
        }
    }

}
