using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crypto.Entities
{
    [MessagePackObject]
    public class EthereumTransaction
    {
        [Key(0)] public string hash { get; set; }
        [Key(1)] public string from { get; set; }
        [Key(2)] public string to { get; set; }
        [Key(3)] public int isError { get; set; }
    }
}
