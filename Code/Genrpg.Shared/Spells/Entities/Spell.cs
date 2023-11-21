using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Spells.Interfaces;

namespace Genrpg.Shared.Spells.Entities
{
    public class SpellFlags
    {
        public const int FoundItem = 1 << 0;
        public const int InstantHit = 1 << 1;
        public const int IsPassive = 1 << 2;
    }

    [MessagePackObject]
    public class Spell : OwnerPlayerData, ISpell
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }

        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long ElementTypeId { get; set; }
        [Key(8)] public int Range { get; set; }
        [Key(9)] public float CastTime { get; set; }
        [Key(10)] public long PowerStatTypeId { get; set; }
        [Key(11)] public int PowerCost { get; set; }
        [Key(12)] public int Cooldown { get; set; }
        [Key(13)] public int MaxCharges { get; set; }
        [Key(14)] public int Shots { get; set; }

        [Key(15)] public DateTime CooldownEnds { get; set; }
        [Key(16)] public int CurrCharges { get; set; }

        [Key(17)] public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [Key(18)] public List<SpellEffect> Effects { get; set; } = new List<SpellEffect>();

        public Spell()
        {
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

        public float GetCooldownSeconds(GameState gs, Unit caster)
        {
            if (caster == null)
            {
                return Cooldown;
            }

            return Cooldown * (1 - caster.Stats.Pct(StatTypes.Cooldown));
        }

        public int GetCost(GameState gs, Unit caster)
        {
            if (caster == null)
            {
                return PowerCost;
            }

            return (int)Math.Ceiling((float)(PowerCost * (1 - caster.Stats.Pct(StatTypes.Efficiency))));
        }
    }
}
