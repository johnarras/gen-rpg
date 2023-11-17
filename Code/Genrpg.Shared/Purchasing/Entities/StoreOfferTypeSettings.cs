using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
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
        [Key(4)] public long TotalModSize { get; set; }
        [Key(5)] public long MaxAcceptableModValue { get; set; }
        [Key(6)] public long Priority { get; set; }
        [Key(7)] public long StoreSlotTypeId { get; set; }
        [Key(8)] public long StoreFeatureTypeId { get; set; }
        [Key(9)] public long ProductSetTypeId { get; set; }

        [Key(10)] public double MinDaysSinceInstall { get; set; }
        [Key(11)] public double MaxDaysSinceInstall { get; set; }
        [Key(12)] public long MinLevel { get; set; }
        [Key(13)] public long MaxLevel { get; set; }
        [Key(14)] public long SpendTimes { get; set; }
        [Key(15)] public double TotalSpend { get; set; }

        [Key(16)] public bool UseDateRange { get; set; }
        [Key(17)] public DateTime StartDate { get; set; }
        [Key(18)] public DateTime EndDate { get; set; }

    }
}
