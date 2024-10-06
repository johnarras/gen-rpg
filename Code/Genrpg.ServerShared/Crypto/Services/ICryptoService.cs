using Genrpg.ServerShared.Core;
using Genrpg.Shared.Crypto.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Crypto.Services
{
    public interface ICryptoService : IInjectable
    {
        Task<EthereumTransactionList> GetTransactionsFromWallet(string walletAddress, bool internalTransactions);
        string GetPasswordHash(string salt, string passwordOrToken);
        string GetRandomBytes(); 
        string QuickHash(string txt);
    }
}
