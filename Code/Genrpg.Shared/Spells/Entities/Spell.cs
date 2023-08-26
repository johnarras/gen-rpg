using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Spells.Entities
{
    public class SpellFlags
    {
        public const int FoundItem = 1 << 0;
        public const int InstantHit = 1 << 1;
        public const int IsPassive = 1 << 2;
    }

    [MessagePackObject]
    public class Spell : IIndexedGameItem, IStatusItem
    {


        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public string Desc { get; set; }
        [Key(3)] public string Icon { get; set; }
        [Key(4)] public string Art { get; set; }
        [Key(5)] public string StringId { get; set; }

        [Key(6)] public long ElementTypeId { get; set; }
        [Key(7)] public long SkillTypeId { get; set; }

        [Key(8)] public int CastingTime { get; set; }
        [Key(9)] public int Cost { get; set; }
        [Key(10)] public int Cooldown { get; set; }
        [Key(11)] public int Range { get; set; }
        [Key(12)] public int Radius { get; set; }
        [Key(13)] public int Duration { get; set; }
        [Key(14)] public int Shots { get; set; }
        [Key(15)] public int MaxCharges { get; set; }
        [Key(16)] public int ExtraTargets { get; set; }
        [Key(17)] public int Scale { get; set; }
        [Key(18)] public int ComboGen { get; set; }

        [Key(19)] public int FinalScale { get; set; }

        [Key(20)] public long OrigSpellTypeId { get; set; }

        [Key(21)] public List<SpellProc> Procs { get; set; }

        [Key(22)] public DateTime CooldownEnds { get; set; }
        [Key(23)] public int CurrCharges { get; set; }

        [Key(24)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        public Spell()
        {
            FinalScale = 100;
            Procs = new List<SpellProc>();
        }

        public int GetRange()
        {
            return MathUtils.Clamp(SkillType.MinRange, SkillType.MinRange + Range * SkillType.RangePointDistance, SkillType.MaxRange);
        }

        public bool UsesProjectile()
        {
            if (GetRange() < 1)
            {
                return false;
            }

            if (HasFlag(SpellFlags.InstantHit))
            {
                return false;
            }

            return true;
        }

        public bool MatchesOther(Spell other)
        {
            if (other == null ||
                other.SkillTypeId != SkillTypeId ||
                other.ElementTypeId != ElementTypeId)
            {
                return false;
            }

            if (IdKey > 0 && IdKey == other.IdKey)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(StringId) && StringId == other.StringId)
            {
                return true;
            }

            return false;
        }

        public float GetCooldownSeconds(GameState gs, Unit caster)
        {
            if (caster == null)
            {
                return Cooldown;
            }

            return Cooldown * (1 - caster.Stats.Get(StatType.Cooldown, StatCategory.Pct));
        }

        public int GetCost(GameState gs, Unit caster)
        {
            if (caster == null)
            {
                return Cost;
            }

            return (int)Math.Ceiling((float)(Cost * (1 - caster.Stats.Pct(StatType.Efficiency))));
        }
    }
}
