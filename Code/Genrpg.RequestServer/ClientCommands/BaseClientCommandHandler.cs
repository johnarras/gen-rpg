using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.PlayerData.Services;

namespace Genrpg.RequestServer.ClientCommands
{
    public abstract class BaseClientCommandHandler<C> : IClientCommandHandler where C : IClientCommand
    {

        protected IPlayerDataService _playerDataService = null;
        protected ILoginPlayerDataService _loginPlayerDataService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IServerConfig _config = null;
        protected IWebServerService _loginServerService = null;

        protected abstract Task InnerHandleMessage(WebContext context, C command, CancellationToken token);

        public Type GetKey()
        {
            return typeof(C);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IWebCommand command, CancellationToken token)
        {
            await InnerHandleMessage(context, (C)command, token);
        }

        protected void ShowError(WebContext context, string msg)
        {
            context.Results.Add(new ErrorResult() { Error = msg });
        }
    }

}
