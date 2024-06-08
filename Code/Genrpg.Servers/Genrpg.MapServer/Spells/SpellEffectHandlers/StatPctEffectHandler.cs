using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class StatPctEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityTypes.StatPct; }
        public override bool IsModifyStatEffect() { return true; }
        public override bool UseStatScaling() { return false; }

        public override List<ActiveSpellEffect> CreateEffects(IRandom rand, SpellHit hitData)
        {

            List<ActiveSpellEffect> retval = new List<ActiveSpellEffect>();

            long target = hitData.SkillType.TargetTypeId;

            List<StatPct> list = null;
            if (target == TargetTypes.Enemy)
            {
                list = hitData.ElementType.DebuffEffects;
            }
            else
            {
                list = hitData.ElementType.BuffEffects;
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

                ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
                eff.EntityTypeId = EntityTypes.StatPct;
                eff.EntityId = statPct.StatTypeId;
                eff.Quantity = statPct.Percent;
                retval.Add(eff);
            }
            return retval;
        }

        public override bool HandleEffect(IRandom rand, ActiveSpellEffect eff)
        {
            return true;
        }
    }
}
