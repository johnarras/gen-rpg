using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Website.Messages.Error;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.CloudComms.Servers.PlayerServer.Queues;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.ServerShared.CloudComms.Services;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.Accounts.Services;
using Genrpg.Shared.Accounts.PlayerData;
using Genrpg.Shared.Users.PlayerData;
using MongoDB.Driver;
using Genrpg.RequestServer.Services.WebServer;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.RequestServer.PlayerData.Services;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.Shared.Accounts.WebApi.Login;
using Genrpg.RequestServer.AuthRequestHandlers.Constants;

namespace Genrpg.RequestServer.Auth.RequestHandlers
{
    public abstract class BaseAuthRequestHandler<TRequest> : IAuthRequestHandler where TRequest : IAuthRequest
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
        protected ICryptoService _cryptoService = null!;

        protected abstract Task HandleRequestInternal(WebContext context, TRequest request, CancellationToken token);

        public Type GetKey()
        {
            return typeof(TRequest);
        }

        public virtual async Task Reset()
        {
            await Task.CompletedTask;
        }

        public async Task Execute(WebContext context, IAuthRequest request, CancellationToken token)
        {
            await HandleRequestInternal(context, (TRequest)request, token);
        }
        protected void ShowError(WebContext context, string msg)
        {
            context.Responses.Add(new ErrorResponse() { Error = msg });
        }

        protected async Task AfterAuthSuccess(WebContext context, Account account, IAuthLoginRequest request, EAuthResponse authResponse)
        {
            await context.LoadUser(account.Id);

            context.user.SessionId = HashUtils.NewGuid();
            context.user.ClientVersion = request.ClientVersion;
            await _repoService.Save(context.user);

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.DeviceId);

            string clientLoginToken = null;
            if (authRecord == null || authRecord.TokenExpiry < DateTime.UtcNow || authResponse == EAuthResponse.UsedPassword)
            {
                if (authRecord == null)
                {
                    authRecord = new AuthRecord()
                    {
                        DeviceId = request.DeviceId,
                    };
                    account.AuthRecords.Add(authRecord);
                }
                clientLoginToken = _cryptoService.GetRandomBytes();
                authRecord.TokenSalt = _cryptoService.GetRandomBytes();
                authRecord.TokenHash = _cryptoService.GetPasswordHash(authRecord.TokenSalt, clientLoginToken);
                authRecord.TokenExpiry = DateTime.UtcNow.AddDays(7);
                await _repoService.Save(account);
            }

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = context.user.Id, Name = "User" + context.user.Id });

            LoginResponse loginResponse = new LoginResponse()
            {
                User = SerializationUtils.ConvertType<User, User>(context.user),
                LoginToken = clientLoginToken,
                CharacterStubs = await _playerDataService.LoadCharacterStubs(context.user.Id),
                MapStubs = _webServerService.GetMapStubs().Stubs,
                UserData = await _loginPlayerDataService.LoadPlayerDataOnLogin(context, null)
            };

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            List<ITopLevelSettings> topLevelSettings = _gameDataService.GetClientGameData(context.user, true);
            loginResponse.GameData = _gameDataService.MapToApi(context.user, topLevelSettings);

            context.Responses.Add(loginResponse);

            _accountService.AddAccountToProductGraph(account, request.AccountProductId, request.ReferrerId);

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

        protected EAuthResponse ExistingPasswordIsOk(Account account, IAuthLoginRequest request)
        {
            string newPasswordHash = _cryptoService.GetPasswordHash(account.PasswordSalt, request.Password);

            if (newPasswordHash == account.PasswordHash)
            {
                return EAuthResponse.UsedPassword;
            }

            AuthRecord authRecord = account.AuthRecords.FirstOrDefault(x => x.DeviceId == request.DeviceId);

            if (authRecord == null)
            {
                return EAuthResponse.Failure;
            }

            string newTokenHash = _cryptoService.GetPasswordHash(authRecord.TokenSalt, request.Password);

            if (newTokenHash == authRecord.TokenHash)
            {
                return EAuthResponse.UsedToken;
            }
            else
            {
                return EAuthResponse.Failure;
            }
        }
    }
}
