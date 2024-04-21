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
    public interface ICryptoService : IInitializable
    {
        Task<EthereumTransactionList> GetTransactionsFromWallet(ServerGameState gs, string walletAddress, bool internalTransactions);
    }
}
