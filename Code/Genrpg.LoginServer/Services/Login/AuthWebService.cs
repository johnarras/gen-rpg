using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Services.LoginServer;
using Genrpg.LoginServer.Utils;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Purchasing.Services;
using Genrpg.ServerShared.Utils;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Achievements.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Constants;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Login;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Genrpg.LoginServer.AuthCommandHandlers;

namespace Genrpg.LoginServer.Services.Login
{
    public class AuthWebService : IAuthWebService
    {
        private IWebServerService _webServerService = null;

        public async Task HandleAuthCommand(WebContext context, string postData, CancellationToken token)
        {
            WebServerCommandSet commandSet = SerializationUtils.Deserialize<WebServerCommandSet>(postData);

            foreach (IAuthCommand authCommand in commandSet.Commands)
            {
                IAuthCommandHandler handler = _webServerService.GetAuthCommandHandler(authCommand.GetType());

                if (handler != null)
                {
                    await handler.Execute(context, authCommand, token);
                }
            }
        }
    }
}

