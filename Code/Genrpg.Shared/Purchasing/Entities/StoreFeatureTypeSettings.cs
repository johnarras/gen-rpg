using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class StoreFeatureTypeSettings : ParentSettings<StoreFeatureType>
    {
        [Key(0)] public override string Id { get; set; }

        public StoreFeatureType GetStoreFeatureType(long idkey) { return _lookup.Get<StoreFeatureType>(idkey); }
    }

    [MessagePackObject]
    public class StoreFeatureTypeSettingsApi : ParentSettingsApi<StoreFeatureTypeSettings, StoreFeatureType> { }
    [MessagePackObject]
    public class StoreFeatureTypeSettingsLoader : ParentSettingsLoader<StoreFeatureTypeSettings, StoreFeatureType, StoreFeatureTypeSettingsApi> { }


    [MessagePackObject]
    public class StoreFeatureType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Desc { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public double DollarPrice { get; set; }
        [Key(7)] public long GemPrice { get; set; }
        [Key(8)] public string GoogleProductId { get; set; }
        [Key(9)] public string AppleProductId { get; set; }
    }
}
