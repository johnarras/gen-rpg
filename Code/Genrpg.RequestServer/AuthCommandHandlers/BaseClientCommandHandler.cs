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
using Genrpg.Shared.Users.PlayerData;
using Genrpg.ServerShared.Utils;
using MongoDB.Driver;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.RequestServer.PlayerData.Services;

namespace Genrpg.RequestServer.AuthCommandHandlers
{
    public abstract class BaseAuthCommandHandler<C> : IAuthCommandHandler where C : IAuthCommand
    {

        protected IPlayerDataService _playerDataService = null!;
        protected ILoginPlayerDataService _loginPlayerDataService = null!;
        protected ILogService _logService = null!;
        protected IRepositoryService _repoService = null!;
        protected IServerConfig _config = null!;
        protected IWebServerService _loginServerService = null!;
        protected IGameDataService _gameDataService = null!;
        protected ICloudCommsService _cloudCommsService = null!;
        protected IWebServerService _webServerService = null!;
        protected IAccountService _accountService = null!;

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

        protected async Task AfterAuthSuccess(WebContext context, Account account, IAuthLoginCommand command)
        {
            await context.LoadUser(account.Id);

            context.user.SessionId = HashUtils.NewGuid();
            context.user.ClientVersion = command.ClientVersion;
            await _repoService.Save(context.user);

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == command.DeviceId);

            string clientLoginToken = null;
            if (authRecord == null || authRecord.TokenExpiry < DateTime.UtcNow)
            {
                if (authRecord == null)
                {
                    authRecord = new AuthRecord()
                    {
                        DeviceId = command.DeviceId,
                    };
                    account.AuthRecords.Add(authRecord);
                }
                clientLoginToken = PasswordUtils.GetRandomBytes();
                authRecord.TokenSalt = PasswordUtils.GetRandomBytes();
                authRecord.TokenHash = PasswordUtils.GetPasswordHash(authRecord.TokenSalt, clientLoginToken);
                authRecord.TokenExpiry = DateTime.UtcNow.AddDays(7);
                await _repoService.Save(account);
            }
            
            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = context.user.Id, Name = "User" + context.user.Id });

            LoginResult loginResult = new LoginResult()
            {
                User = SerializationUtils.ConvertType<User, User>(context.user),
                LoginToken = clientLoginToken,
                CharacterStubs = await _playerDataService.LoadCharacterStubs(context.user.Id),
                MapStubs = _webServerService.GetMapStubs().Stubs,
                UserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null)           };

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            List<ITopLevelSettings> topLevelSettings = _gameDataService.GetClientGameData(context.user, true);
            loginResult.GameData = _gameDataService.MapToApi(context.user,topLevelSettings);

            context.Results.Add(loginResult);

            _accountService.AddAccountToProductGraph(account, command.AccountProductId, command.ReferrerId);

            UpdatePublicData(account, context.user);

        }


        private void UpdatePublicData(Account account, User user)
        {
            // Just always make new files and save them.

            PublicAccount publicAccount = new PublicAccount() { Id = account.Id };

            publicAccount.Name = account.ShareId;
            _repoService.QueueSave(publicAccount);

            PublicUser publicUser = new PublicUser() { Id = user.Id };
            publicUser.Name = account.ShareId;
            _repoService.QueueSave(publicUser);

        }

        protected bool ExistingPasswordIsOk(Account account, IAuthLoginCommand command)
        {
            string newPasswordHash = PasswordUtils.GetPasswordHash(account.PasswordSalt, command.Password);

            if (newPasswordHash == account.PasswordHash)
            {
                return true;
            }

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x=>x.DeviceId == command.DeviceId);  

            if (authRecord == null)
            {
                return false;
            }

            string newTokenHash = PasswordUtils.GetPasswordHash(authRecord.TokenSalt, command.Password);

            return newTokenHash == authRecord.TokenHash; 
        }
    }
}
