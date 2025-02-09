using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class CompletedPurchaseData : BasePlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string UserId { get; set; }
        [Key(2)] public DateTime Date { get; set; }
        [Key(3)] public string ReceiptHash { get; set; }
        [Key(4)] public string ReceiptData { get; set; }
    }
}
