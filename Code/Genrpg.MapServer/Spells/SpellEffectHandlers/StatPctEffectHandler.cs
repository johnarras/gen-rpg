using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Stats.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class StatPctEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityType.StatPct; }
        public override bool IsModifyStatEffect() { return true; }
        public override bool UseStatScaling() { return false; }

        public override List<SpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {

            List<SpellEffect> retval = new List<SpellEffect>();

            long target = hitData.SendSpell.SkillType.TargetTypeId;

            List<StatPct> list = null;
            if (target == TargetType.Enemy)
            {
                list = hitData.SendSpell.ElementType.DebuffEffects;
            }
            else
            {
                list = hitData.SendSpell.ElementType.BuffEffects;
            }

            if (list == null)
            {
                return retval;
            }

            foreach (StatPct statPct in list)
            {
                if (statPct.StatTypeId == 0 || statPct.Percent == 0)
                {
                    continue;
                }

                SpellEffect eff = new SpellEffect(hitData);
                eff.EntityTypeId = EntityType.StatPct;
                eff.EntityId = statPct.StatTypeId;
                eff.Quantity = statPct.Percent;
                retval.Add(eff);
            }
            return retval;
        }

        public override bool HandleEffect(GameState gs, SpellEffect eff)
        {
            return true;
        }
    }
}
