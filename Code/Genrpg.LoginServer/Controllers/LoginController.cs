using Genrpg.ServerShared.Accounts;
using Genrpg.Shared.Accounts.Constants;
using Genrpg.Shared.Accounts.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Login.Constants;
using Genrpg.Shared.Users.Entities;
using Genrpg.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.ServerShared.CloudMessaging.Services;
using System.Security.Cryptography.Xml;
using Genrpg.Shared.Login.Messages.Login;
using Genrpg.ServerShared.PlayerData;
using Genrpg.ServerShared.Maps;
using Genrpg.LoginServer.Core;
using Genrpg.Shared.Login.Interfaces;
using Genrpg.Shared.Login.Messages;
using Genrpg.Shared.Versions.Entities;
using System.Threading;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Messages;
using ZstdSharp.Unsafe;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.GameSettings;
using Genrpg.Shared.GameSettings.Entities;
using Genrpg.Shared.DataStores.Categories;
using Microsoft.AspNetCore.Routing.Constraints;
using Genrpg.Shared.GameSettings.Loading;

namespace Genrpg.LoginServer.Controllers
{




    [Route("[controller]")]
    [ApiController]
    public class LoginController : BaseWebController
    {

        private AccountService _accountService;
        private static IGameDataService _gameDataService;
        public LoginController() : base()
        {
            _accountService = new AccountService();
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Login" };
        }

        [HttpPost]
        public async Task<string> Post([FromForm] string Data)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            try
            {
                await SetupGameState(cts.Token);

                if (_gameDataService == null)
                {
                    _gameDataService = gs.loc.Get<IGameDataService>();
                }
                // Do this by hand since we have no userId/sessionId at this point
                
                LoginServerCommandSet commands = SerializationUtils.Deserialize<LoginServerCommandSet>(Data);

                LoginCommand loginData = commands.Commands.FirstOrDefault() as LoginCommand;

                await Login(loginData);

                return PackageResults(gs.Results);
            }
            catch (Exception e)
            {
                ShowError(gs, "Login Exception: " + e.Message + "\n" + e.StackTrace);
                return PackageResults(gs.Results);
            }
        }



        public async Task Login(LoginCommand loginData)
        {

            if (string.IsNullOrEmpty(loginData.Password))
            {
                ShowError(gs, "Missing password");
                return;
            }

            Account account = await _accountService.LoadBy(gs.config, AccountSearch.Email, loginData.Email);

            if (account != null)
            {
                if (account.HasFlag(AccountFlags.Banned))
                {
                    ShowError(gs, "You have been banned");
                    return;
                }

                if (string.IsNullOrEmpty(account.PasswordSalt))
                {
                    ShowError(gs, "Account data was incorret");
                    return;
                }

                string hashedPassword = _accountService.GetPasswordHash(account.PasswordSalt, loginData.Password);

                if (hashedPassword != account.Password)
                {
                    ShowError(gs, "Incorrect UserId or Password");
                    return;
                }
            }
            else
            {
                string passwordError = await PasswordError(loginData.Password);

                if (!string.IsNullOrEmpty(passwordError))
                {
                    ShowError(gs, passwordError);
                    return;
                }

                string nameError = await ScreenNameError(loginData.Email, loginData.Email);
                if (!string.IsNullOrEmpty(nameError))
                {
                    ShowError(gs, nameError);
                    return;
                }

                string emailError = await EmailError(loginData.Email, loginData.Email);

                if (!string.IsNullOrEmpty(emailError))
                {
                    ShowError(gs, emailError);
                    return;
                }

                account = new Account()
                {
                    Name = loginData.Name,
                    Email = loginData.Email,
                    PasswordSalt = _accountService.GeneratePasswordSalt(),
                };

                account.Password = _accountService.GetPasswordHash(account.PasswordSalt, loginData.Password);

                await _accountService.SaveAccount(gs.config, account);
                account = await _accountService.LoadBy(gs.config, AccountSearch.Email, loginData.Email);

                if (account == null)
                {
                    ShowError(gs, "Failed to save Account");
                }
            }
            User user = await gs.repo.Load<User>(account.Id.ToString());

            user = await CreateOrUpdateUserFromAccount(account, user, loginData);

            await UpdateUserVersion(user);
            await gs.repo.Save(user);

            gs.loc.Get<ICloudMessageService>().SendMessage(CloudServerNames.Player, new LoginUser() { Id = user.Id, Name = user.Name });

            LoginResult loginResult = new LoginResult()
            {
                User = SerializationUtils.ConvertType<User, User>(user),
                CharacterStubs = await PlayerDataUtils.LoadCharacterStubs(gs, user.Id),
                MapStubs = gs.mapStubs,
            };


            List<IGameSettingsLoader> loaders = _gameDataService.GetAllLoaders();

            loginResult.GameData = _gameDataService.GetClientData(gs, gs.user, true);

            gs.Results.Add(loginResult);
        }

        private async Task<User> CreateOrUpdateUserFromAccount(Account acct, User user, LoginCommand loginData)
        {
            if (user == null)
            {
                user = new User();
                user.AddFlags(UserFlags.SoundActive | UserFlags.MusicActive);
            }
            user.Id = acct.Id.ToString();
            user.Name = acct.Name;
            user.Email = acct.Email;
            user.SessionId = HashUtils.NewGuid();
            gs.user = user;

            await gs.repo.Save(user);
            User u2 = await gs.repo.Load<User>(user.Id);

            return u2;
        }

        protected async Task<string> EmailError(string email, string networkName)
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
                Account acct = await _accountService.LoadBy(gs.config, AccountSearch.Email, email);
                if (acct != null)
                {
                    return "This email is already in use.";
                }
            }
            return "";
        }

        protected async Task<string> ScreenNameError(string screenname, string networkName)
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
                Task<Account> acct = _accountService.LoadBy(gs.config, AccountSearch.Name, screenname);
                if (acct != null)
                {
                    return "This name is already in use.";
                }
            }
            await Task.CompletedTask;
            return "";
        }

        public virtual async Task<string> PasswordError(string password)
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


        public virtual async Task UpdateUserVersion(User u)
        {
            if (gs.data.GetGameData<VersionSettings>(u).UserVersion <= u.Version)
            {
                return;
            }

            await Task.CompletedTask;
        }
    }
}
