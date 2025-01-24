
using Genrpg.Shared.Utils;
using Genrpg.ServerShared.MainServer;
using Genrpg.ServerShared.CloudComms.Constants;
using Genrpg.ServerShared.Crypto.Services;
using Genrpg.Shared.Crypto.Entities;
using System.Text;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.RequestServer.Services.Clients;
using Genrpg.RequestServer.Services.Login;
using Genrpg.RequestServer.Setup;
using Genrpg.RequestServer.Services.NoUsers;
using Genrpg.Shared.Website.Messages;
using Genrpg.RequestServer.RequestHandlers;

namespace Genrpg.RequestServer.Core
{
    /// <summary>
    /// This is a minimal amount of webdev used to get us into code that can be used elsewhere easier.
    /// </summary>
    public class WebRequestServer : BaseServer<WebContext, WebsiteSetupService, IWebRequestHandler>
    {
        protected IClientWebService _clientWebService { get; private set; }
        protected IAuthWebService _authWebService { get; private set; }
        protected ICryptoService _cryptoService { get; private set; }
        protected ICharmService _charmService { get; private set; }
        protected INoUserWebService _noUserWebService { get; private set; }
        protected IRepositoryService _repositoryService { get; private set; }
        private CancellationTokenSource _serverSource = new CancellationTokenSource();
        protected CancellationToken _token => _serverSource.Token;

        public WebRequestServer()
        {
            _serverSource = new CancellationTokenSource();

            Init(null, null, _serverSource.Token).Wait();
            _clientWebService = _context.loc.Get<IClientWebService>();
            _authWebService = _context.loc.Get<IAuthWebService>();
            _cryptoService = _context.loc.Get<ICryptoService>();
            _charmService = _context.loc.Get<ICharmService>();
            _noUserWebService = _context.loc.Get<INoUserWebService>();
        }

        protected WebContext SetupContext()
        {
            return new WebContext(_config, _context.loc);
        }

        protected string _serverInstanceId = CloudServerNames.Login + HashUtils.NewGuid().ToString().ToLowerInvariant();
        protected override string GetServerId(object data)
        {
            return _serverInstanceId;
        }

        public async Task<string> HandleUserClient(string postData)
        {
            WebContext context = SetupContext();
            await _clientWebService.HandleUserClientRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleNoUser(string postData)
        {
            WebContext context = SetupContext();
            await _noUserWebService.HandleNoUserRequest(context, postData, _token);
            return PackageResponses(context);
        }

        public async Task<string> HandleAuth(string postData)
        {
            WebContext context = SetupContext();
            await _authWebService.HandleAuthRequest(context, postData, _token);
            return PackageResponses(context);
        }

        private string PackageResponses(WebContext context)
        {
            return SerializationUtils.Serialize(new WebServerResponseSet() { Responses = context.Responses });
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

                    List<string> bonusTexts = _charmService.PrintBonuses(blist);

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
