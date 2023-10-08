using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class UnitEffectHandler : BaseSpellEffectHandler
    {
        public override long GetKey() { return EntityType.Unit; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return false; }

        public override List<SpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            List<SpellEffect> retval = new List<SpellEffect>();

            ElementSkill elemSkill = hitData.SendSpell.ElementType.GetSkill(hitData.SendSpell.SkillType.IdKey);

            if (elemSkill == null || elemSkill.OverrideEntityTypeId != EntityType.Unit)
            {
                return retval;
            }

            SpellEffect eff = new SpellEffect(hitData);
            eff.EntityTypeId = EntityType.Unit;
            eff.EntityId = elemSkill.OverrideEntityId;
            eff.Quantity = elemSkill.ScalePct;


            retval.Add(eff);
            return retval;
        }

        public override bool HandleEffect(GameState gs, SpellEffect eff)
        {

            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ))
            {
                return false;
            }

            MyPointF pos = targ.GetPos();
            pos.Z += 2;

            int statPct = (int)eff.Quantity;

            UnitGenData unitGenData = new UnitGenData()
            {
                UnitTypeId = eff.EntityId,
                Level = eff.Level,
                FactionTypeId = targ.FactionTypeId,
                Pos = pos,
                StatPct = statPct,
            };

            return true;
        }
    }
}
