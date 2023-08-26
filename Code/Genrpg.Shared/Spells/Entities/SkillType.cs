using MessagePack;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

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
    public class SkillType : IIndexedGameItem, IInfo
    {

        public const int DefaultBuffLevelScale = 10;

        public const int MinRange = 5;
        public const int MaxRange = 45;

        public const int RangePointDistance = 10;

        
        [Key(0)] public long IdKey { get; set; }
        public long GetId() { return IdKey; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }


        /// <summary>
        /// Skill or spell always uses one power pool for its effects. This is on the SKill, then modified
        /// by the source.
        /// </summary>
        [Key(5)] public long PowerStatTypeId { get; set; }
        /// <summary>
        /// Cost of the skill. modified by the type of source used for this.
        /// </summary>
        [Key(6)] public int PowerCost { get; set; }

        /// <summary>
        /// Enemy, Ally or None.
        /// </summary>
        [Key(7)] public long TargetTypeId { get; set; }

        /// <summary>
        /// Which stat is used for scaling of this (this cascades into defenses and multipliers as well
        /// </summary>
        [Key(8)] public long ScalingStatTypeId { get; set; }


        /// <summary>
        /// Overall scaling percent of the final stat+mult calculated above.
        /// In the case of non heal/dam spells this is ignored.
        /// </summary>
        [Key(9)] public int BaseScalePct { get; set; }

        /// <summary>
        /// Scaling pct added to dam/heals and base points per level for buff/debuff/summon.
        /// </summary>
        [Key(10)] public int RankScale { get; set; }

        [Key(11)] public long EntityTypeId { get; set; }
        [Key(12)] public List<AbilityEffect> BonusEfffects { get; set; }


        [Key(13)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


        /// <summary>
        /// How much the output of this skill is allowed to vary when cast.
        /// </summary>
        [Key(14)] public int VariancePct { get; set; }


        public SkillType()
        {
            BaseScalePct = 100;
            RankScale = DefaultBuffLevelScale;
            BonusEfffects = new List<AbilityEffect>();
        }

        public string ShowInfo()
        {
            return "Skill: " + Name;
        }
        public bool HasTarget()
        {
            return TargetTypeId == TargetType.Enemy || TargetTypeId == TargetType.Ally;
        }
    }

}
