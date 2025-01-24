using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    public class CompletedPurchaseData : BasePlayerData
    {
        public override string Id { get; set; }
        public string UserId { get; set; }
        public DateTime Date { get; set; }
        public string ReceiptHash { get; set; }
        public string ReceiptData { get; set; }
    }
}
