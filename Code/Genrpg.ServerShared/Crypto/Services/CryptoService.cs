using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crypto.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.WebRequests.Utils;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Security.Cryptography;
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

        public string GetPasswordHash(string salt, string passwordOrToken)
        {
            if (string.IsNullOrEmpty(passwordOrToken) || string.IsNullOrEmpty(salt))
            {
                return "";
            }

            string txt2 = salt + passwordOrToken;

            return PasswordHash(txt2);
        }

        public string GetRandomBytes()
        {
            byte[] buff = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(buff);
        }

        public string QuickHash(string txt)
        {
            MD5 algo = MD5.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }

        private string PasswordHash(string txt)
        {
            // For now to avoid adding keygen lib...stronger hashes don't work always too.
            SHA256 algo = SHA256.Create();
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(txt);
            byte[] arr2 = algo.ComputeHash(arr);
            return Convert.ToBase64String(arr2);
        }
    }
}
