using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crypto.Entities
{
    [MessagePackObject]
    public class EthereumTransactionList
    {
        [Key(0)] public string WalletAddress { get; set; }
        [Key(1)] public string Message { get; set; }
        [Key(2)] public List<EthereumTransaction> result { get; set; } = new List<EthereumTransaction>();
    }
}
