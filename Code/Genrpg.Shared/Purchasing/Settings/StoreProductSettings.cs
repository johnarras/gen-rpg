using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreProductSettings : ParentSettings<StoreProduct>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StoreProductSettingsApi : ParentSettingsApi<StoreProductSettings, StoreProduct> { }
    [MessagePackObject]
    public class StoreProductSettingsLoader : ParentSettingsLoader<StoreProductSettings, StoreProduct, StoreProductSettingsApi> 
    {
        public override bool SendToClient() { return false; }
    }

    [MessagePackObject]
    public class StoreProduct : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public List<SpawnItem> Rewards { get; set; }
    }
}
