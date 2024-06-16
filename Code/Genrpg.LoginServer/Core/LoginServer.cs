using Genrpg.LoginServer.Services.Clients;
using Genrpg.LoginServer.Services.Login;
using Genrpg.LoginServer.Setup;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.GameSettings;
using System.Threading.Tasks;
using System.Threading;
using Genrpg.Shared.Utils;
using Genrpg.LoginServer.Utils;
using System.ComponentModel;
using Genrpg.ServerShared.MainServer;
using System;
using Genrpg.LoginServer.MessageHandlers;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.Shared.Setup.Services;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.Shared.Crypto.Entities;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Genrpg.Shared.Stats.Entities;
using System.Text;
using Genrpg.Shared.Stats.Constants;
using Microsoft.Win32.SafeHandles;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Charms.Settings;
using System.Linq;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.LoginServer.Services.NoUsers;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.LoginServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class LoginServer : BaseServer<LoginContext, LoginSetupService, ILoginMessageHandler>
    {
        protected IClientService _clientService { get; private set; }
        protected ILoginService _loginService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected INoUserService _noUserService { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;

        public LoginServer()
        {
            _serverSource = new CancellationTokenSource();

            Init(null, null, _serverSource.Token).Wait();
            _clientService = _context.loc.Get<IClientService>();
            _loginService = _context.loc.Get<ILoginService>();
            _cryptoService = _context.loc.Get<ICryptoService>();
            _charmService = _context.loc.Get<ICharmService>();
            _noUserService = _context.loc.Get<INoUserService>();
        }

        protected LoginContext SetupContext()
        {
            return new LoginContext(_config)
            {
                loc = _context.loc,
                rand = new MyRandom(),
            };
        }

        protected string _serverInstanceId = CloudServerNames.Login + HashUtils.NewGuid().ToString().ToLowerInvariant();
        protected override string GetServerId(object data)
        {
            return _serverInstanceId;
        }

        public async Task<string> HandleClient(string postData)
        {
            LoginContext context = SetupContext(); 
            await _clientService.HandleClient(context, postData, _token);
            return WebUtils.PackageResults(context.Results);
        }

        public async Task<string> HandleNoUser(string postData)
        {
            LoginContext context = SetupContext();
           await _noUserService.HandleNoUserCommand(context, postData, _token);
            return WebUtils.PackageResults(context.Results);
        }

        public async Task<string> HandleLogin(string postData)
        {
            LoginContext context = SetupContext();
            await _loginService.Login(context, postData, _token);
            return WebUtils.PackageResults(context.Results);
        }

        public async Task<string> HandleTxList(string address)
        {
            MyRandom rand = new MyRandom();
            EthereumTransactionList normalList = await _cryptoService.GetTransactionsFromWallet(address, false);

            EthereumTransactionList internalList = await _cryptoService.GetTransactionsFromWallet(address, true);

            List<EthereumTransaction> allTransactions = new List<EthereumTransaction>(normalList.result);
            allTransactions.AddRange(internalList.result);

            StringBuilder retval = new StringBuilder();
            retval.Append("EXAMPLE CONVERTING TRANSACTIONS INTO STAT BONUSES: NOT FINAL TUNING\n\n");

            foreach (EthereumTransaction trans in allTransactions)
            {
                retval.Append("TX: " + trans.hash + "\n");

                List<PlayerCharmBonusList> list = _charmService.CalcBonuses(trans.hash);

                foreach (PlayerCharmBonusList blist in list)
                {

                    List<String> bonusTexts = _charmService.PrintBonuses(blist);

                    foreach (string btext in bonusTexts)
                    {
                        retval.AppendLine("    " + btext);
                    }
                }
            }

            return retval.ToString();
        }
    }
}
