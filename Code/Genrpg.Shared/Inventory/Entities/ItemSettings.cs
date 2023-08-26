using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.GameDatas;
using Genrpg.Shared.Stats.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemSettings : BaseGameData
    {
        public override void Set(GameData gameData) { gameData.Set(this); }
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int GenSameStatPercent { get; set; }
        [Key(2)] public int GenSameStatBonusPct { get; set; }
        [Key(3)] public int GenGlobalScalingPercent { get; set; }

        [Key(4)] public List<EquipSlot> EquipSlots { get; set; }
        [Key(5)] public List<ItemType> ItemTypes { get; set; }
        [Key(6)] public List<SetType> SetTypes { get; set; }
        [Key(7)] public List<ScalingType> ScalingTypes { get; set; }
        [Key(8)] public List<QualityType> QualityTypes { get; set; }

        public EquipSlot GetEquipSlot(long idkey)
        {
            return _lookup.Get<EquipSlot>(idkey);
        }

        public ItemType GetItemType(long idkey)
        {
            return _lookup.Get<ItemType>(idkey);
        }

        public SetType GetSetType(long idkey)
        {
            return _lookup.Get<SetType>(idkey);
        }

        public ScalingType GetScalingType(long idkey)
        {
            return _lookup.Get<ScalingType>(idkey);
        }

        public QualityType GetQualityType(long idkey)
        {
            return _lookup.Get<QualityType>(idkey);
        }



        private List<ItemType> _primaryReagents = null;
        private List<ItemType> _secondaryReagents = null;

        /// <summary>
        /// All items that have the 1 bit set in their flags (0x01 == ItemType.PrimaryReagent)
        /// </summary>
        /// <returns></returns>
        public List<ItemType> GetPrimaryReagents()
        {
            if (_primaryReagents == null)
            {
                _primaryReagents = ItemTypes.Where(x => x.IsReagent() && x.HasFlag(ItemType.PrimaryReagent)).ToList();
            }
            return _primaryReagents;
        }

        public List<ItemType> GetSecondaryReagents()
        {
            if (_secondaryReagents == null)
            {
                _secondaryReagents = ItemTypes.Where(x => x.IsReagent() && !x.HasFlag(ItemType.PrimaryReagent)).ToList();
            }
            return _secondaryReagents;
        }

    }
}
