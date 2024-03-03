using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreFeatureSettings : ParentSettings<StoreFeature>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StoreFeatureSettingsApi : ParentSettingsApi<StoreFeatureSettings, StoreFeature> { }
    [MessagePackObject]
    public class StoreFeatureSettingsLoader : ParentSettingsLoader<StoreFeatureSettings, StoreFeature, StoreFeatureSettingsApi> { }


    [MessagePackObject]
    public class StoreFeature : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
    }
}
