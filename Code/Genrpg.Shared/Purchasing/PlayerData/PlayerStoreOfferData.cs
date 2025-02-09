using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Purchasing.Settings;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Genrpg.Shared.Purchasing.PlayerData
{
    [MessagePackObject]
    public class PlayerStoreOffer
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string OfferId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Art { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public long StoreFeatureId { get; set; }
        [Key(7)] public long StoreSlotId { get; set; }
        [Key(8)] public long StoreThemeId { get; set; }
        [Key(9)] public DateTime EndDate { get; set; }

        [Key(10)] public List<PlayerStoreOfferItem> Items { get; set; } = new List<PlayerStoreOfferItem>();
    }
    
    [MessagePackObject]
    public class PlayerStoreOfferItem
    {
        [Key(0)] public long Index { get; set; }
        [Key(1)] public StoreItem StoreItem { get; set; }
        [Key(2)] public ProductSku Sku { get; set; }
        [Key(3)] public string UniqueStoreItemId { get; set; }
    }



    [MessagePackObject]
    public class PlayerStoreOfferData : NoChildPlayerData, IUserData, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public DateTime GameDataSaveTime { get; set; } = DateTime.UtcNow;

        [Key(2)] public DateTime LastTimeSet { get; set; } = DateTime.MinValue;

        [Key(3)] public List<PlayerStoreOffer> StoreOffers { get; set; } = new List<PlayerStoreOffer>();
    }

    [MessagePackObject]
    public class PlayerStoreOfferDataMapper : UnitDataMapper<PlayerStoreOfferData> { }


    [MessagePackObject]
    public class CurrentStoresLoader : UnitDataLoader<PlayerStoreOfferData> { }
}
