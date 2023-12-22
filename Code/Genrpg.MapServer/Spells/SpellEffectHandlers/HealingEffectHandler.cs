using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public class HealingEffectHandler : HealthEffectHandler
    {
        public override long GetKey() { return EntityTypes.Healing; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }


        public override List<ActiveSpellEffect> CreateEffects(GameState gs, SpellHit hitData)
        {
            ActiveSpellEffect eff = new ActiveSpellEffect(hitData);
            eff.EntityTypeId = EntityTypes.Healing;
            eff.Quantity = hitData.BaseQuantity;
            return new List<ActiveSpellEffect>() { eff };
        }

        public override bool HandleEffect(GameState gs, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                eff.SetCancelled(true);
                return false;
            }

            eff.CurrQuantity = eff.Quantity;

            int variancePct = 20;
                eff.CurrQuantity = MathUtils.LongRange(eff.Quantity * (100 - variancePct) / 100,
                    eff.Quantity * (100 + variancePct) / 100, gs.rand);

            if (eff.Quantity != 0 && _objectManager.GetChar(eff.CasterId, out Character ch))
            {
                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.TotalHealing, eff.Quantity);
                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.MaxHealing, eff.Quantity);
            }


            return base.HandleEffect(gs, eff);
        }
    }
}
