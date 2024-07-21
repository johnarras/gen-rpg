using Genrpg.LoginServer.Core;
using Genrpg.LoginServer.Services.LoginServer;
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
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Website.Messages.Login;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.Shared.Accounts.PlayerData;

namespace Genrpg.LoginServer.AuthCommandHandlers
{
    public abstract class BaseAuthCommandHandler<C> : IAuthCommandHandler where C : IAuthCommand
    {

        protected IPlayerDataService _playerDataService = null;
        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IServerConfig _config = null;
        protected IWebServerService _loginServerService = null;
        protected IGameDataService _gameDataService = null;
        protected ICloudCommsService _cloudCommsService = null;
        protected IWebServerService _webServerService = null;
        protected IAccountService _accountService = null;

        protected abstract Task InnerHandleMessage(WebContext context, C command, CancellationToken token);

        public Type GetKey()
        {
            return typeof(C);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IAuthCommand command, CancellationToken token)
        {
            await InnerHandleMessage(context, (C)command, token);
        }
        protected void ShowError(WebContext context, string msg)
        {
            context.Results.Add(new ErrorResult() { Error = msg });
        }

        protected async Task AfterAuthSuccess(WebContext context, Account account, long accountProductId, string referrerId)
        {
            User user = await _repoService.Load<User>(account.Id);
            user.SessionId = HashUtils.NewGuid();
            await _repoService.Save(user);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = user.Id, Name = "User" + user.Id });

            LoginResult loginResult = new LoginResult()
            {
                User = SerializationUtils.ConvertType<User, User>(user),
                CharacterStubs = await _playerDataService.LoadCharacterStubs(user.Id),
                MapStubs = _webServerService.GetMapStubs().Stubs,
            };

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            loginResult.GameData = _gameDataService.GetClientGameData(null, true);

            context.Results.Add(loginResult);

            _accountService.AddAccountToProductGraph(account, accountProductId, referrerId);
        }
    }
}
