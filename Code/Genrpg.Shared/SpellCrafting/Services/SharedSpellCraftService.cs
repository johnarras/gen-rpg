using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.SpellCrafting.Constants;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using Genrpg.Shared.Spells.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.Stats.Settings.Stats;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.SpellCrafting.Services
{
    public class SharedSpellCraftService : ISharedSpellCraftService
    {
        private Dictionary<long, ISpellModifierHelper> _modifierHelpers = null;

        public virtual async Task Setup(GameState gs, CancellationToken token)
        {           
            _modifierHelpers = ReflectionUtils.SetupDictionary<long, ISpellModifierHelper>(gs);
            await Task.CompletedTask;
        }

        public ISpellModifierHelper GetSpellModifierHelper(long modifierId)
        {
            if (_modifierHelpers.TryGetValue(modifierId, out ISpellModifierHelper helper))
            {
                return helper;
            }

            return null;
        }            

        public bool ValidateSpellData(GameState gs, MapObject obj, ISpell spell)
        {
            if (spell == null)
            {
                return false;
            }

            ElementType elemType = gs.data.Get<ElementTypeSettings>(null).Get(spell.ElementTypeId);

            if (elemType == null)
            {
                return false;
            }

            double spellCostScale = 1.0f;
            double costPercent = 0;
            string firstSkillName = "";

            ISpellModifierHelper cooldownHelper = GetSpellModifierHelper(SpellModifiers.Cooldown);
            spell.Cooldown = (int)(cooldownHelper.GetValidValue(gs, obj, spell.Cooldown));
            spellCostScale *= cooldownHelper.GetCostScale(gs, obj, spell.Cooldown);

            ISpellModifierHelper castTimeHelper = GetSpellModifierHelper(SpellModifiers.CastTime);
            spell.CastTime = (float)(castTimeHelper.GetValidValue(gs,obj,spell.CastTime));
            spellCostScale *= castTimeHelper.GetCostScale(gs,obj, spell.CastTime);

            ISpellModifierHelper rangeHelper = GetSpellModifierHelper(SpellModifiers.Range);
            spell.MaxRange = (int)(rangeHelper.GetValidValue(gs, obj, spell.MaxRange));
            spellCostScale *= rangeHelper.GetCostScale(gs, obj, spell.MaxRange);

            ISpellModifierHelper shotsHelper = GetSpellModifierHelper(SpellModifiers.Shots);
            spell.Shots = (int)(shotsHelper.GetValidValue(gs, obj, spell.Shots));
            spellCostScale *= shotsHelper.GetCostScale(gs, obj, spell.Shots);

            ISpellModifierHelper maxChargesHelper = GetSpellModifierHelper(SpellModifiers.MaxCharges);
            spell.MaxCharges = (int)(maxChargesHelper.GetValidValue(gs, obj, spell.MaxCharges));
            spellCostScale *= maxChargesHelper.GetCostScale(gs, obj, spell.MaxCharges);

            foreach (SpellEffect effect in spell.Effects)
            {

                double effectCostScale = 1.0f;

                SkillType skillType = gs.data.Get<SkillTypeSettings>(obj).Get(effect.SkillTypeId);

                if (skillType == null)
                {
                    return false;
                }

                if (effect.Radius != 0)
                {
                    ISpellModifierHelper radiusHelper = GetSpellModifierHelper(SpellModifiers.Radius);
                    effect.Radius = (int)radiusHelper.GetValidValue(gs, obj, effect.Radius);
                    effectCostScale *= radiusHelper.GetCostScale(gs, obj, effect.Radius);
                }

                if (effect.Duration > 0)
                {
                    ISpellModifierHelper durationHelper = GetSpellModifierHelper(SpellModifiers.Duration);
                    effect.Duration = (int)durationHelper.GetValidValue(gs, obj, effect.Duration);
                    effectCostScale *= durationHelper.GetCostScale(gs, obj, effect.Duration);
                }

                if (effect.ExtraTargets > 0)
                {
                    ISpellModifierHelper extraTargetsHelper = GetSpellModifierHelper(SpellModifiers.ExtraTargets);
                    effect.ExtraTargets = (int)extraTargetsHelper.GetValidValue(gs, obj, effect.ExtraTargets);
                    effectCostScale *= extraTargetsHelper.GetCostScale(gs, obj, effect.ExtraTargets);
                }

                if (effect.Scale != 100)
                {
                    ISpellModifierHelper scaleHelper = GetSpellModifierHelper(SpellModifiers.Scale);
                    effect.Scale = (int)scaleHelper.GetValidValue(gs, obj, effect.Scale);
                    effectCostScale *= scaleHelper.GetCostScale(gs, obj, effect.Scale);
                }


                firstSkillName = skillType.Name;

                long currentCostPercent = 100;
                currentCostPercent = skillType.GetCostPercentFromPowerStat(spell.PowerStatTypeId);

                costPercent += currentCostPercent * effectCostScale;
            }

            costPercent *= spellCostScale;

            StatType powerStatType = gs.data.Get<StatSettings>(null).Get(spell.PowerStatTypeId);

            long oldPowerCost = spell.PowerCost;
            spell.PowerCost = (int)(costPercent * powerStatType.MaxPool / 100.0f);

            if (oldPowerCost != spell.PowerCost)
            {
                spell.SetDirty(true);
            }

            if (string.IsNullOrEmpty(spell.Name))
            {
                spell.Name = elemType.Name + " " + firstSkillName;
            }

            return true;
        }
    }
}
