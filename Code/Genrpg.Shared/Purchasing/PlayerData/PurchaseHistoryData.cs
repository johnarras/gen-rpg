using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Interfaces;

namespace Genrpg.Shared.Purchasing.PlayerData
{

    public class PurchaseHistoryLoader : UnitDataLoader<PurchaseHistoryData> { }

    public class PurchaseHistoryMapper : UnitDataMapper<PurchaseHistoryData> { }

    [MessagePackObject]
    public class PurchaseHistoryData : NoChildPlayerData, IUserData, IServerOnlyData
    {
        public const int MaxRecentPurchasesCount = 10;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double PurchaseTotal { get; set; }
        [Key(2)] public long PurchaseCount { get; set; }
        [Key(3)] public DateTime FirstPurchase { get; set; }
        [Key(4)] public DateTime LatestPurchase { get; set; }

        [Key(5)] public List<RecentPurchase> RecentPurchases { get; set; } = new List<RecentPurchase>();
    }

    [MessagePackObject]
    public class RecentPurchase
    {
        [Key(0)] public DateTime PurchaseTime { get; set; }
        [Key(1)] public double Price { get; set; }
        [Key(2)] public long ProductSkuId { get; set; }
        [Key(3)] public long StoreItemId { get; set; }
        [Key(4)] public long StoreSlotId { get; set; }
        [Key(5)] public string Name { get; set; }
    }
}
