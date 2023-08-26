using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Names.Entities;
using Genrpg.Shared.ProcGen.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class ItemType : IIndexedGameItem
    {

        public const int MinRangedItemLevel = 5;
        public const int LevelGap = 2 * MinRangedItemLevel;

        public const int PrimaryReagent = 1 << 0; // 1
        public const int FlagTwoHandedItem = 1 << 1; // 2
        public const int NoStack = 1 << 2; // 4
        public const int SkipScalingIconName = 1 << 3; // 8

        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string NameId { get; set; }
        [Key(3)] public string GenName { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        // Probably want to use bitfields but bleh. IDK.
        [Key(7)] public long EquipSlotId { get; set; }

        [Key(8)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


        [Key(9)] public List<ItemEffect> Effects { get; set; }

        [Key(10)] public List<LevelRangeName> LevelRanges { get; set; }

        [Key(11)] public List<QualityName> QualityNames { get; set; }

        [Key(12)] public List<NameCount> IconCounts { get; set; }

        [Key(13)] public List<WeightedName> Names { get; set; }

        public ItemType()
        {
            Effects = new List<ItemEffect>();
            LevelRanges = new List<LevelRangeName>();
            QualityNames = new List<QualityName>();
            IconCounts = new List<NameCount>();
            Names = new List<WeightedName>();
        }


        public long GetRangedLevel(GameState gs, long desiredLevel)
        {
            if (LevelRanges == null || LevelRanges.Count < 1)
            {
                return desiredLevel;
            }


            LevelRangeName levelRangeWanted = LevelRanges.FirstOrDefault(x => x.MinLevel <= desiredLevel && x.MaxLevel >= desiredLevel);
            if (levelRangeWanted == null)
            {
                return MinRangedItemLevel;
            }

            return (levelRangeWanted.MinLevel + levelRangeWanted.MaxLevel) / 2;
        }

        public LevelRangeName GetRange(GameState gs, long level, int depth = 0)
        {
            if (depth >= 10)
            {
                return null;
            }
            if (LevelRanges != null && LevelRanges.Count > 0)
            {
                LevelRangeName ln = LevelRanges.FirstOrDefault(x => x.MinLevel <= level && x.MaxLevel >= level);
                if (ln != null)
                {
                    return ln;
                }
            }
            return null;
        }

        public QualityName GetQuality(GameState gs, int quality, int depth = 0)
        {
            if (depth >= 10)
            {
                return null;
            }

            if (QualityNames != null && QualityNames.Count > 0)
            {
                QualityName qn = QualityNames.FirstOrDefault(x => x.QualityTypeId == quality);
                if (qn != null)
                {
                    return qn;
                }
            }
            return null;
        }


        public string GetIcon(GameState gs, long level)
        {
            if (LevelRanges == null)
            {
                return Icon;
            }
            LevelRangeName ln = LevelRanges.FirstOrDefault(x => x.MinLevel <= level && x.MaxLevel >= level);
            if (ln != null && !string.IsNullOrEmpty(ln.Icon))
            {
                return ln.Icon;
            }
            return Icon;
        }
        public string GetNamePrefix(GameState gs, long level, long quality)
        {
            return "";
        }

        public bool IsReagent()
        {
            return CanStack() &&
                Effects != null &&
                Effects.Count > 0;
        }

        public Dictionary<long, long> GetCraftingStatPercents(GameState gs, long level, long quality)
        {
            Dictionary<long, long> dict = new Dictionary<long, long>();

            if (Effects == null)
            {
                return dict;
            }

            LevelData ldata = gs.data.GetGameData<LevelSettings>().GetLevel(level);
            QualityType qtype = gs.data.GetGameData<ItemSettings>().GetQualityType(quality);

            int baseStat = 10;
            int qualityPercent = 100;
            if (ldata != null)
            {
                baseStat = ldata.StatAmount;
            }

            if (qtype != null)
            {
                qualityPercent = qtype.ItemStatPct;
            }

            int globalScaling = gs.data.GetGameData<ItemSettings>().GenGlobalScalingPercent;


            foreach (ItemEffect eff in Effects)
            {
                if (eff.EntityTypeId != EntityType.Stat)
                {
                    continue;
                }


                // amt    basestat globalscale qualScale example
                // 125   *   75    *    20    *   150 =  
                long value = eff.Quantity * baseStat * globalScaling * qualityPercent / (100L * 100);
                dict[eff.EntityId] = value;
            }

            return dict;
        }

        /// <summary>
        /// Get any slots related by parent/child relationships to this slot. Go only one level deep.
        /// </summary>
        /// <param name="gs"></param>
        /// <returns></returns>
        public List<long> GetRelatedEquipSlots(GameState gs)
        {
            List<long> retval = new List<long>();
            if (gs.data.GetGameData<ItemSettings>().EquipSlots == null || EquipSlotId < 1)
            {
                return retval;
            }


            EquipSlot eqSlot = gs.data.GetGameData<ItemSettings>().GetEquipSlot(EquipSlotId);
            if (eqSlot == null)
            {
                return retval;
            }
            retval.Add(EquipSlotId);

            if (eqSlot.ParentEquipSlotId > 0 && !retval.Contains(eqSlot.ParentEquipSlotId))
            {
                retval.Add(eqSlot.ParentEquipSlotId);
            }

            List<EquipSlot> childSlots = gs.data.GetGameData<ItemSettings>().EquipSlots.Where(x => x.ParentEquipSlotId == EquipSlotId).ToList();
            foreach (EquipSlot childSlot in childSlots)
            {
                if (!retval.Contains(childSlot.IdKey))
                {
                    retval.Add(childSlot.IdKey);
                }
            }
            return retval;

        }

        /// <summary>
        /// Get all slots where we could place this item.
        /// </summary>
        /// <param name="gs"></param>
        /// <returns></returns>
        public List<long> GetCompatibleEquipSlots(GameState gs)
        {
            List<long> retval = new List<long>();
            if (gs.data.GetGameData<ItemSettings>().EquipSlots == null || EquipSlotId < 1)
            {
                return retval;
            }

            EquipSlot eqSlot = gs.data.GetGameData<ItemSettings>().GetEquipSlot(EquipSlotId);
            if (eqSlot == null)
            {
                return retval;
            }
            retval.Add(EquipSlotId);

            if (EquipSlotId == EquipSlot.OffHand)
            {
                return retval;
            }

            if (HasFlag(FlagTwoHandedItem))
            {
                return retval;
            }


            long mainSlotId = eqSlot.ParentEquipSlotId > 0 ? eqSlot.ParentEquipSlotId : eqSlot.IdKey;

            foreach (EquipSlot slot in gs.data.GetGameData<ItemSettings>().EquipSlots)
            {
                if (slot.IdKey < 1 || retval.Contains(slot.IdKey))
                {
                    continue;
                }
                if (slot.IdKey == mainSlotId || slot.ParentEquipSlotId == mainSlotId)
                {
                    retval.Add(slot.IdKey);
                }

            }
            return retval;
        }

        public bool CanStack()
        {
            return EquipSlotId < 1;
        }

    }
}
