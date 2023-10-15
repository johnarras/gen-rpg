using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemTypeSettings : ParentSettings<ItemType>
    {
        [Key(0)] public int GenSameStatPercent { get; set; }
        [Key(1)] public int GenSameStatBonusPct { get; set; }
        [Key(2)] public int GenGlobalScalingPercent { get; set; }
        [Key(3)] public override string Id { get; set; }

        public ItemType GetItemType(long idkey) { return _lookup.Get<ItemType>(idkey); }

        List<ItemType> _primaryReagents = null;
        public List<ItemType> GetPrimaryReagents()
        {
            if (_primaryReagents == null)
            {
                _primaryReagents = _data.Where(x => x.IsReagent() && x.HasFlag(ItemFlags.PrimaryReagent)).ToList();
            }
            return _primaryReagents;
        }

        List<ItemType> _secondaryReagents = null;
        public List<ItemType> GetSecondaryReagents()
        {
            if (_secondaryReagents == null)
                {
                    _secondaryReagents = _data.Where(x => x.IsReagent() && !x.HasFlag(ItemFlags.PrimaryReagent)).ToList();
                }
                return _secondaryReagents;
            }
    }

    [MessagePackObject]
    public class ItemTypeSettingsApi : ParentSettingsApi<ItemTypeSettings, ItemType> { }
    [MessagePackObject]
    public class ItemTypeSettingsLoader : ParentSettingsLoader<ItemTypeSettings, ItemType, ItemTypeSettingsApi> { }

}
