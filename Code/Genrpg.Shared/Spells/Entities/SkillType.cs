using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Stats.Constants;

namespace Genrpg.Shared.Spells.Entities
{
    public class SkillFlags
    {
        public const int DisableRanks = 1 << 0;
    }

    /// <summary>
    /// This class is for the overall skills the user can learn in broad categories.
    /// </summary>
    [MessagePackObject]
    public class SkillType : ChildSettings, IIndexedGameItem, IInfo
    {

        public const int DefaultBuffLevelScale = 10;

        public const int MinRange = 5;
        public const int MaxRange = 45;

        public const int RangePointDistance = 5;

        public const long DefaultCostPercent = 50;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        /// <summary>
        /// Enemy, Ally or None.
        /// </summary>
        [Key(7)] public long TargetTypeId { get; set; }

        [Key(8)] public long ManaCostPercent { get; set; }
        [Key(9)] public long EnergyCostPercent { get; set; }
        [Key(10)] public long ComboCostPercent { get; set; }

        [Key(11)] public long ScalingStatTypeId { get; set; }

        /// <summary>
        /// Overall scaling percent of the final stat+mult calculated above.
        /// In the case of non heal/dam spells this is ignored.
        /// </summary>
        [Key(12)] public int StatScalePercent { get; set; }

        [Key(13)] public long EffectEntityTypeId { get; set; }

        public long GetId() { return IdKey; }

        public SkillType()
        {
            StatScalePercent = 100;
        }

        public string ShowInfo()
        {
            return "Skill: " + Name;
        }
        public bool HasTarget()
        {
            return TargetTypeId == TargetTypes.Enemy || TargetTypeId == TargetTypes.Ally;
        }

        public long GetCostPercentFromPowerStat(long powerStatTypeId)
        {
            if (powerStatTypeId == StatTypes.Mana)
            {
                return ManaCostPercent;
            }
            else if (powerStatTypeId == StatTypes.Energy)
            {
                return EnergyCostPercent;
            }
            else if (powerStatTypeId == StatTypes.Combo)
            {
                return ComboCostPercent;
            }
            return DefaultCostPercent;
        }
    }

}
