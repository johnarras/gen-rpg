using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
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


        public override long GetKey() { return EntityTypes.Damage; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<ActiveSpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Damage;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(GameState gs, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }

            long startAmount = eff.Quantity;
            long amount = eff.Quantity;

            int variancePct = 20;
            
                amount = MathUtils.LongRange(startAmount * (100 - variancePct) / 100,
                    startAmount * (100 + variancePct) / 100, gs.rand);

            long absorbAmount = 0;
            bool isImmune = targ.IsFullImmune(gs);

            if (isImmune)
            {
                amount = 0;
            }
            if (amount != 0 && _objectManager.GetChar(eff.CasterId, out Character ch))
            {

                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.TotalDamage, amount);
                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.MaxDamage, amount);
            }

            amount = -amount;

            if (targ.SpellEffects == null)
            {
                targ.SpellEffects = new List<ActiveSpellEffect>();
            }
            List<ActiveSpellEffect> shields = targ.SpellEffects.Where(x => x.EntityTypeId == EntityTypes.Shield).ToList();

            foreach (ActiveSpellEffect shield in shields)
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
