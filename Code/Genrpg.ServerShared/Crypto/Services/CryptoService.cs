using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crypto.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.WebRequests.Utils;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Crypto.Services
{

    public class CryptoService : ICryptoService
    {
        private IServerConfig _config = null;

        public async Task<EthereumTransactionList> GetTransactionsFromWallet(string address, bool internalTransactions)
        {
            if (string.IsNullOrEmpty(address))
            {
                return new EthereumTransactionList();
            }

            string action = (internalTransactions ? "txlistinternal" : "txlist");

            string myapikey = _config.EtherscanKey;

            string url = "https://api.etherscan.io/api?module=account&action=" + action + "&address=" + address;
            url += "&sort=desc&apikey=" + myapikey;

            try
            {

                byte[] response = await WebRequestUtils.DownloadBytes(url);

                EthereumTransactionList list = SerializationUtils.Deserialize<EthereumTransactionList>(System.Text.UTF8Encoding.UTF8.GetString(response));

                list.result = list.result.Where(x => x.isError == 0).ToList();

                list.WalletAddress = address;
                return list;
            }
            catch (Exception ee)
            {
                Console.WriteLine("Exc: " + ee.Message);
            }
            return new EthereumTransactionList();

        }
    }
}
