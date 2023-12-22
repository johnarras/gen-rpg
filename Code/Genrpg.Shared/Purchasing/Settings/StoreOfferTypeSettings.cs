using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Purchasing.PlayerData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreOfferTypeSettings : ParentSettings<StoreOfferType>
    {
        [Key(0)] public override string Id { get; set; }

        public StoreOfferType GetStoreOfferType(long idkey) { return _lookup.Get<StoreOfferType>(idkey); }
    }

    [MessagePackObject]
    public class StoreOfferTypeSettingsApi : ParentSettingsApi<StoreOfferTypeSettings, StoreOfferType> { }
    [MessagePackObject]
    public class StoreOfferTypeSettingsLoader : ParentSettingsLoader<StoreOfferTypeSettings, StoreOfferType, StoreOfferTypeSettingsApi> { }


    [MessagePackObject]
    public class StoreOfferType : ChildSettings, IPlayerFilter
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long TotalModSize { get; set; }
        [Key(8)] public long MaxAcceptableModValue { get; set; }
        [Key(9)] public long Priority { get; set; }
        [Key(10)] public long StoreSlotTypeId { get; set; }
        [Key(11)] public long StoreFeatureTypeId { get; set; }

        [Key(12)] public double MinUserDaysSinceInstall { get; set; }
        [Key(13)] public double MaxUserDaysSinceInstall { get; set; }
        [Key(14)] public double MinCharDaysSinceInstall { get; set; }
        [Key(15)] public double MaxCharDaysSinceInstall { get; set; }
        [Key(16)] public long MinLevel { get; set; }
        [Key(17)] public long MaxLevel { get; set; }
        [Key(18)] public long MinPurchaseCount { get; set; }
        [Key(19)] public long MaxPurchaseCount { get; set; }
        [Key(20)] public double MinPurchaseTotal { get; set; }
        [Key(21)] public double MaxPurchaseTotal { get; set; }

        [Key(22)] public bool UseDateRange { get; set; }
        [Key(23)] public DateTime StartDate { get; set; }
        [Key(24)] public DateTime EndDate { get; set; }

        [Key(25)] public List<OfferProduct> Products { get; set; } = new List<OfferProduct>();

    }
}
