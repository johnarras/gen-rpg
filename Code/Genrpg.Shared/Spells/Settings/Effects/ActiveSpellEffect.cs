using MessagePack;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Spells.Settings.Effects
{
    [MessagePackObject]
    public sealed class ActiveSpellEffect : BaseMapApiMessage, IDisplayEffect
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long Quantity { get; set; }
        [Key(3)] public long EntityId { get; set; }
        [Key(4)] public string CasterId { get; set; }
        [Key(5)] public string CasterGroupId { get; set; }
        [Key(6)] public string TargetId { get; set; }
        [Key(7)] public long Level { get; set; }
        [Key(8)] public long SpellId { get; set; }
        [Key(9)] public string Icon { get; set; }
        [Key(10)] public float MaxDuration { get; set; } // 0 = infinite duration
        [Key(11)] public float DurationLeft { get; set; }
        [Key(12)] public bool IsOrigTarget { get; set; }
        [Key(13)] public long Radius { get; set; }
        [Key(14)] public long ElementTypeId { get; set; }
        [Key(15)] public long SkillTypeId { get; set; }
        [Key(16)] public long Range { get; set; }
        [Key(17)] public float CritChance { get; set; }
        [Key(18)] public float CritMult { get; set; }
        [Key(19)] public long CurrQuantity { get; set; }
        [Key(20)] public long CasterFactionId { get; set; }
        public ActiveSpellEffect()
        {

        }

        public ActiveSpellEffect(SpellHit hit)
        {
            Level = hit.SendSpell.CasterLevel;
            CasterId = hit.SendSpell.CasterId;
            CasterGroupId = hit.SendSpell.CasterGroupId;
            TargetId = hit.Target.Id;
            SpellId = hit.SendSpell.Spell.IdKey;
            Icon = hit.SendSpell.Spell.Icon;
            MaxDuration = hit.Effect.Duration;
            DurationLeft = MaxDuration;
            Radius = hit.Effect.Radius;
            IsOrigTarget = hit.OrigTarget == hit.Target;
            ElementTypeId = hit.SendSpell.Spell.ElementTypeId;
            SkillTypeId = hit.Effect.SkillTypeId;
            Range = hit.SendSpell.Spell.GetRange();
            CritChance = hit.CritChance;
            CritMult = hit.CritMult;
            CasterFactionId = hit.SendSpell.CasterFactionId;
        }

        public bool MatchesOther(IDisplayEffect eff)
        {
            if (eff is ActiveSpellEffect activeSpellEffect)
            {

                return CasterId == activeSpellEffect.CasterId &&
                    SpellId == activeSpellEffect.SpellId;
            }
            return false;
        }
    }
}
