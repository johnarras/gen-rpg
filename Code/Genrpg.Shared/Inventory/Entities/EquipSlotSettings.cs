using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class EquipSlotSettings : ParentSettings<EquipSlot>
    {
        [Key(0)] public override string Id { get; set; }

        public EquipSlot GetEquipSlot(long idkey) { return _lookup.Get<EquipSlot>(idkey); }
    }

    [MessagePackObject]
    public class EquipSlotSettingsApi : ParentSettingsApi<EquipSlotSettings, EquipSlot> { }
    [MessagePackObject]
    public class EquipSlotSettingsLoader : ParentSettingsLoader<EquipSlotSettings, EquipSlot, EquipSlotSettingsApi> { }

}
