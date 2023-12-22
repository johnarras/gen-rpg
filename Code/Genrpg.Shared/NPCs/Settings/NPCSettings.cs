using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;

namespace Genrpg.Shared.NPCs.Settings
{
    [MessagePackObject]
    public class NPCSettings : ParentSettings<NPCType>
    {
        [Key(0)] public override string Id { get; set; }

        public NPCType GetNPCType(long idkey) { return _lookup.Get<NPCType>(idkey); }
    }

    [MessagePackObject]
    public class NPCType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long CrafterTypeId { get; set; }
        [Key(8)] public long BuildingTypeId { get; set; }
        [Key(9)] public List<VendorItem> DefaultVendorItems { get; set; } = new List<VendorItem>();

    }

    [MessagePackObject]
    public class NPCSettingsApi : ParentSettingsApi<NPCSettings, NPCType> { }

    [MessagePackObject]
    public class NPCSettingsLoader : ParentSettingsLoader<NPCSettings, NPCType, NPCSettingsApi> { }

}
