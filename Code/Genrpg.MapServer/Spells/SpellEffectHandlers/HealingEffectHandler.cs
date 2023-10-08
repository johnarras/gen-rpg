using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class HealingEffectHandler : HealthEffectHandler
    {
        public override long GetKey() { return EntityType.Healing; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<SpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            SpellEffect eff = new SpellEffect(hitData);
            eff.EntityTypeId = EntityType.Healing;
            eff.Quantity = hitData.BaseQuantity;
            return new List<SpellEffect>() { eff };
        }

        public override bool HandleEffect(GameState gs, SpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                eff.SetCancelled(true);
                return false;
            }

            eff.CurrQuantity = eff.Quantity;

            if (eff.VariancePct > 0 && eff.VariancePct <= 100)
            {
                eff.CurrQuantity = MathUtils.LongRange(eff.Quantity * (100 - eff.VariancePct) / 100,
                    eff.Quantity * (100 + eff.VariancePct) / 100, gs.rand);
            }

            return base.HandleEffect(gs, eff);
        }
    }
}
