using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spells.Constants;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.Settings.Effects;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.SpellEffectHandlers
{
    public abstract class HealthEffectHandler : BaseSpellEffectHandler
    {


        public override long GetKey() { return -1; }
        public override bool IsModifyStatEffect() { return false; }
        public override bool UseStatScaling() { return true; }
        public override float GetTickLength() { return SpellConstants.DotTickSeconds; }

        public override bool HandleEffect(GameState gs, ActiveSpellEffect eff)
        {
            if (!_objectManager.GetUnit(eff.TargetId, out Unit targ) || targ.HasFlag(UnitFlags.IsDead))
            {
                return false;
            }

            bool isCrit = false;
            long quantity = eff.CurrQuantity;
            if (gs.rand.NextDouble() < eff.CritChance)
            {
                isCrit = true;
                quantity = (long)(quantity * eff.CritMult);
            }

            if (quantity < 0)
            {

                if (targ as Character != null)
                {
                    _spellService.ShowCombatText(gs, targ, quantity.ToString(), CombatTextColors.Red, isCrit);
                }
                else
                {
                    int textColorId = eff.SpellId == 1 ? CombatTextColors.White : CombatTextColors.Yellow;

                    _spellService.ShowCombatText(gs, targ, quantity.ToString(), textColorId, isCrit);

                }
            }
            else if (quantity > 0)
            {
                _spellService.ShowCombatText(gs, targ, quantity.ToString(), CombatTextColors.Green, isCrit);
            }

            _statService.Add(targ, StatTypes.Health, StatCategories.Curr, quantity);
            if (targ.Stats.Curr(StatTypes.Health) <= 0)
            {
                _unitService.CheckForDeath(gs, eff, targ);
            }

            return true;
        }
    }
}
