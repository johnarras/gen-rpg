using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class DamageEffectHandler : HealthEffectHandler
    {
        public override long GetKey() { return EntityType.Damage; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<SpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            SpellEffect eff = new SpellEffect(hitData);
            eff.EntityTypeId = EntityType.Damage;
            eff.Quantity = hitData.BaseQuantity;
            return new List<SpellEffect>() { eff };
        }

        public override bool HandleEffect(GameState gs, SpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }

            long startAmount = eff.Quantity;
            long amount = eff.Quantity;

            if (eff.VariancePct > 0 && eff.VariancePct <= 100)
            {
                amount = MathUtils.LongRange(startAmount * (100 - eff.VariancePct) / 100,
                    startAmount * (100 + eff.VariancePct) / 100, gs.rand);
            }

            long absorbAmount = 0;
            bool isImmune = targ.IsFullImmune(gs);

            if (isImmune)
            {
                amount = 0;
            }
            amount = -amount;
            if (targ.SpellEffects == null)
            {
                targ.SpellEffects = new List<SpellEffect>();
            }
            List<SpellEffect> shields = targ.SpellEffects.Where(x => x.EntityTypeId == EntityType.Shield).ToList();

            foreach (SpellEffect shield in shields)
            {
                long currAbsorb = Math.Min(-amount, shield.Quantity);
                amount += currAbsorb;
                shield.Quantity -= currAbsorb;
                if (shield.Quantity <= 0)
                {
                    targ.SpellEffects.Remove(shield);
                    shield.SetCancelled(true);
                }
                absorbAmount += currAbsorb;
                if (amount <= 0)
                {
                    break;
                }
            }
            eff.CurrQuantity = amount;
            return base.HandleEffect(gs, eff);
        }
    }
}
