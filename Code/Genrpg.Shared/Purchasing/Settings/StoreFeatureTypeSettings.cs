using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
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
    public class StoreFeatureType : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
    }
}
