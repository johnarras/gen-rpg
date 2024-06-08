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
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Spells.PlayerData.Spells;

namespace Genrpg.Shared.SpellCrafting.Services
{
    public class SharedSpellCraftService : ISharedSpellCraftService
    {
        private IGameData _gameData = null;
        private IRepositoryService _repoService = null;   
        private Dictionary<long, ISpellModifierHelper> _modifierHelpers = null;

        public virtual async Task Initialize(IGameState gs, CancellationToken token)
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

        public bool ValidateSpellData(MapObject obj, ISpell spell)
        {
            if (spell == null)
            {
                return false;
            }

            ElementType elemType = _gameData.Get<ElementTypeSettings>(null).Get(spell.ElementTypeId);

            if (elemType == null)
            {
                return false;
            }

            double spellCostScale = 1.0f;
            double costPercent = 0;
            string firstSkillName = "";

            ISpellModifierHelper cooldownHelper = GetSpellModifierHelper(SpellModifiers.Cooldown);
            spell.Cooldown = (int)(cooldownHelper.GetValidValue( obj, spell.Cooldown));
            spellCostScale *= cooldownHelper.GetCostScale( obj, spell.Cooldown);

            ISpellModifierHelper castTimeHelper = GetSpellModifierHelper(SpellModifiers.CastTime);
            spell.CastTime = (float)(castTimeHelper.GetValidValue(obj,spell.CastTime));
            spellCostScale *= castTimeHelper.GetCostScale(obj, spell.CastTime);

            ISpellModifierHelper rangeHelper = GetSpellModifierHelper(SpellModifiers.Range);
            spell.MaxRange = (int)(rangeHelper.GetValidValue( obj, spell.MaxRange));
            spellCostScale *= rangeHelper.GetCostScale( obj, spell.MaxRange);

            ISpellModifierHelper shotsHelper = GetSpellModifierHelper(SpellModifiers.Shots);
            spell.Shots = (int)(shotsHelper.GetValidValue( obj, spell.Shots));
            spellCostScale *= shotsHelper.GetCostScale( obj, spell.Shots);

            ISpellModifierHelper maxChargesHelper = GetSpellModifierHelper(SpellModifiers.MaxCharges);
            spell.MaxCharges = (int)(maxChargesHelper.GetValidValue( obj, spell.MaxCharges));
            spellCostScale *= maxChargesHelper.GetCostScale( obj, spell.MaxCharges);

            foreach (SpellEffect effect in spell.Effects)
            {

                double effectCostScale = 1.0f;

                SkillType skillType = _gameData.Get<SkillTypeSettings>(obj).Get(effect.SkillTypeId);

                if (skillType == null)
                {
                    return false;
                }

                if (effect.Radius != 0)
                {
                    ISpellModifierHelper radiusHelper = GetSpellModifierHelper(SpellModifiers.Radius);
                    effect.Radius = (int)radiusHelper.GetValidValue( obj, effect.Radius);
                    effectCostScale *= radiusHelper.GetCostScale( obj, effect.Radius);
                }

                if (effect.Duration > 0)
                {
                    ISpellModifierHelper durationHelper = GetSpellModifierHelper(SpellModifiers.Duration);
                    effect.Duration = (int)durationHelper.GetValidValue( obj, effect.Duration);
                    effectCostScale *= durationHelper.GetCostScale( obj, effect.Duration);
                }

                if (effect.ExtraTargets > 0)
                {
                    ISpellModifierHelper extraTargetsHelper = GetSpellModifierHelper(SpellModifiers.ExtraTargets);
                    effect.ExtraTargets = (int)extraTargetsHelper.GetValidValue( obj, effect.ExtraTargets);
                    effectCostScale *= extraTargetsHelper.GetCostScale( obj, effect.ExtraTargets);
                }

                if (effect.Scale != 100)
                {
                    ISpellModifierHelper scaleHelper = GetSpellModifierHelper(SpellModifiers.Scale);
                    effect.Scale = (int)scaleHelper.GetValidValue( obj, effect.Scale);
                    effectCostScale *= scaleHelper.GetCostScale( obj, effect.Scale);
                }


                firstSkillName = skillType.Name;

                long currentCostPercent = 100;
                currentCostPercent = skillType.GetCostPercentFromPowerStat(spell.PowerStatTypeId);

                costPercent += currentCostPercent * effectCostScale;
            }

            costPercent *= spellCostScale;

            StatType powerStatType = _gameData.Get<StatSettings>(null).Get(spell.PowerStatTypeId);

            long oldPowerCost = spell.PowerCost;
            spell.PowerCost = (int)(costPercent * powerStatType.MaxPool / 100.0f);

            if (oldPowerCost != spell.PowerCost)
            {
                if (spell is Spell realSpell)
                {
                    _repoService.QueueSave(realSpell);
                }
            }

            if (string.IsNullOrEmpty(spell.Name))
            {
                spell.Name = elemType.Name + " " + firstSkillName;
            }

            return true;
        }
    }
}
