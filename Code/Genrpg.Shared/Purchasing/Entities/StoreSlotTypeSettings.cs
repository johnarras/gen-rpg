using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Entities
{
    [MessagePackObject]
    public class StoreSlotTypeSettings : ParentSettings<StoreSlotType>
    {
        [Key(0)] public override string Id { get; set; }

        public StoreSlotType GetStoreSlotType(long idkey) { return _lookup.Get<StoreSlotType>(idkey); }
    }

    [MessagePackObject]
    public class StoreSlotTypeSettingsApi : ParentSettingsApi<StoreSlotTypeSettings, StoreSlotType> { }
    [MessagePackObject]
    public class StoreSlotTypeSettingsLoader : ParentSettingsLoader<StoreSlotTypeSettings, StoreSlotType, StoreSlotTypeSettingsApi> { }


    [MessagePackObject]
    public class StoreSlotType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Icon { get; set; }
    }
}
