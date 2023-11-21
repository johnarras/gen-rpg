
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class StoreProductTypeSettings : ParentSettings<StoreProductType>
    {
        [Key(0)] public override string Id { get; set; }

        public StoreProductType GetStoreProductType(long idkey) { return _lookup.Get<StoreProductType>(idkey); }
    }

    [MessagePackObject]
    public class StoreProductTypeSettingsApi : ParentSettingsApi<StoreProductTypeSettings, StoreProductType> { }
    [MessagePackObject]
    public class StoreProductTypeSettingsLoader : ParentSettingsLoader<StoreProductTypeSettings, StoreProductType, StoreProductTypeSettingsApi> { }

    [MessagePackObject]
    public class StoreProductType : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public List<SpawnItem> Rewards { get; set; }
    }
}
