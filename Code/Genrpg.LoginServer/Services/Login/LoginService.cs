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
using Genrpg.Shared.Accounts.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Login.Constants;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Login.Messages.Error;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.LoginServer.Services.Login
{
    public class LoginService : ILoginService
    {
        private IAccountService _accountService = null;
        private IGameDataService _gameDataService = null;
        private IPlayerDataService _playerDataService = null;
        private ICloudCommsService _cloudCommsService = null;
        private IRepositoryService _repoService = null;
        private ILogService _logService = null;
        private IServerConfig _config = null;
        private ILoginServerService _loginServerService = null;

        public async Task Login(LoginContext context, string postData, CancellationToken token)
        {
            LoginServerCommandSet commandSet = SerializationUtils.Deserialize<LoginServerCommandSet>(postData);

            LoginCommand loginCommand = commandSet.Commands.FirstOrDefault() as LoginCommand;

            if (loginCommand == null)
            {
                WebUtils.ShowError(context, "Login missing LoginCommand");
                return;
            }

            if (string.IsNullOrEmpty(loginCommand.Password))
            {
                WebUtils.ShowError(context, "Missing password");
                return;
            }

            Account account = null;

            if (!string.IsNullOrEmpty(loginCommand.Email))
            {
                account = await _accountService.LoadBy(_config, AccountSearch.Email, loginCommand.Email);
            }
            else if (!string.IsNullOrEmpty(loginCommand.UserId))
            {
                account = await _accountService.LoadBy(_config, AccountSearch.Id, loginCommand.UserId);
            }
            if (account != null)
            {
                if (account.HasFlag(AccountFlags.Banned))
                {
                    WebUtils.ShowError(context, "You have been banned");
                    return;
                }

                if (string.IsNullOrEmpty(account.PasswordSalt))
                {
                    WebUtils.ShowError(context, "Account data was incorret");
                    return;
                }

                string hashedPassword = PasswordUtils.GetPasswordHash(account.PasswordSalt, loginCommand.Password);

                if (hashedPassword != account.Password)
                {
                    WebUtils.ShowError(context, "Incorrect UserId or Password");
                    return;
                }
            }
            else
            {
                string passwordError = await PasswordError(context, loginCommand.Password);

                if (!string.IsNullOrEmpty(passwordError))
                {
                    WebUtils.ShowError(context, passwordError);
                    return;
                }

                string nameError = await ScreenNameError(context, loginCommand.Name, NetworkNames.Username);
                if (!string.IsNullOrEmpty(nameError))
                {
                    WebUtils.ShowError(context, nameError);
                    return;
                }

                string emailError = await EmailError(context, loginCommand.Email, NetworkNames.Username);

                if (!string.IsNullOrEmpty(emailError))
                {
                    WebUtils.ShowError(context, emailError);
                    return;
                }

                account = new Account()
                {
                    Name = loginCommand.Name,
                    Email = loginCommand.Email,
                    PasswordSalt = PasswordUtils.GeneratePasswordSalt(),
                };

                account.Password = PasswordUtils.GetPasswordHash(account.PasswordSalt, loginCommand.Password);

                await _accountService.SaveAccount(_config, account);
                account = await _accountService.LoadBy(_config, AccountSearch.Email, loginCommand.Email);

                if (account == null)
                {
                    WebUtils.ShowError(context, "Failed to save Account");
                }
            }
            User user = await _repoService.Load<User>(account.Id.ToString());

            user = await CreateOrUpdateUserFromAccount(context, account, user, loginCommand);

            await _repoService.Save(user);

            _cloudCommsService.SendQueueMessage(CloudServerNames.Player, new LoginUser() { Id = user.Id, Name = "User" + user.Id });

            LoginResult loginResult = new LoginResult()
            {
                User = SerializationUtils.ConvertType<User, User>(user),
                CharacterStubs = await _playerDataService.LoadCharacterStubs(user.Id),
                MapStubs = _loginServerService.GetMapStubs().Stubs,
            };

            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            loginResult.GameData = _gameDataService.GetClientGameData(context.user, true, loginCommand.ClientSettings);

            context.Results.Add(loginResult);

            return;
        }

        private async Task<User> CreateOrUpdateUserFromAccount(LoginContext context, Account acct, User user, LoginCommand loginData)
        {
            if (user == null)
            {
                user = new User();
                user.AddFlags(UserFlags.SoundActive | UserFlags.MusicActive);
            }
            user.Id = acct.Id.ToString();
            user.SessionId = HashUtils.NewGuid();
            context.user = user;

            await _repoService.Save(user);
            User u2 = await _repoService.Load<User>(user.Id);

            return u2;
        }

        protected async Task<string> EmailError(LoginContext context, string email, string networkName)
        {
            if (string.IsNullOrEmpty(email))
            {
                return "Email cannot be blank";
            }

            int atIndex = email.IndexOf("@");
            int lastDotIndex = email.LastIndexOf(".");
            if (atIndex < 1 || lastDotIndex < 2 ||
                atIndex >= lastDotIndex ||
                lastDotIndex < atIndex + 2 ||
                lastDotIndex >= email.Length - 2)
            {
                return "This doesn't look like a valid email.";
            }

            if (networkName == NetworkNames.Username)
            {
                Account acct = await _accountService.LoadBy(_config, AccountSearch.Email, email);
                if (acct != null)
                {
                    return "This email is already in use.";
                }
            }
            return "";
        }

        protected async Task<string> ScreenNameError(LoginContext context, string screenname, string networkName)
        {
            if (string.IsNullOrEmpty(screenname))
            {
                return "Nickname cannot be blank";
            }

            if (screenname.Length < 3)
            {
                return "Nickname must be at least 3 letters";
            }

            if (screenname.Length > 16)
            {
                return "Nickname must be no more than 16 letters";
            }

            screenname = screenname.ToLower();

            // If this is the username network make sure screen name is unique.
            if (networkName == NetworkNames.Username)
            {
                Account acct = await _accountService.LoadBy(_config, AccountSearch.Name, screenname);
                if (acct != null)
                {
                    return "This name is already in use.";
                }
            }
            await Task.CompletedTask;
            return "";
        }

        public virtual async Task<string> PasswordError(LoginContext context, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "Password cannot be blank";
            }

            int minsize = 7;
            if (password.Length < minsize)
            {
                return "Password must be at least " + minsize + " characters long";
            }

            await Task.CompletedTask;
            return "";
        }
    }
}
