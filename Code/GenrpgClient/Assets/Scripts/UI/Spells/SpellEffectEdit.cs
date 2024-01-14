using Genrpg.Shared.SpellCrafting.Constants;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Skills;
using Genrpg.Shared.Spells.Settings.Spells;
using System;

namespace Assets.Scripts.UI.Spells
{
    public class SpellEffectEdit : BaseBehaviour
    {
        public GDropdown SkillInput;
        public SpellModInputField ScaleInput;
        public SpellModInputField RadiusInput;
        public SpellModInputField DurationInput;
        public SpellModInputField ExtraTargetsInput;
        public GInputField EntityInput;


        private SpellEffect _effect;
        private Spell _spell;
        private SpellbookScreen _screen;

        private Action _onValueChangedAction;

        public void Init(SpellEffect effect, Spell spell, SpellbookScreen screen, Action onValueChangedAction)
        {
            _onValueChangedAction = onValueChangedAction;

            _effect = effect;
            _spell = spell;
            _screen = screen;

            CopyFromEffectToUI();

        }


        public void CopyFromEffectToUI()
        {
            ScaleInput?.Init(SpellModifiers.Scale, _onValueChangedAction);
            ScaleInput?.SetSelectedValue(_effect.Scale);
            RadiusInput?.Init(SpellModifiers.Radius, _onValueChangedAction);
            RadiusInput?.SetSelectedValue(_effect.Radius);
            DurationInput?.Init(SpellModifiers.Duration, _onValueChangedAction);
            DurationInput?.SetSelectedValue(_effect.Duration);
            ExtraTargetsInput?.Init(SpellModifiers.ExtraTargets, _onValueChangedAction);
            ExtraTargetsInput.SetSelectedValue(_effect.ExtraTargets);

            SkillInput?.Init(_gs.data.GetGameData<SkillTypeSettings>(_gs.ch).GetData(), _onValueChangedAction);
            SkillInput.SetFromId(_effect.SkillTypeId);

            _uiService.SetInputText(EntityInput, _effect.EntityId);
        }

        public void CopyFromUIToEffect()
        {
            _effect.SkillTypeId = _uiService.GetSelectedIdFromName(typeof(SkillType), SkillInput);
                
            _effect.Scale = (int)ScaleInput?.GetSelectedValue();
            _effect.Radius = (int)RadiusInput?.GetSelectedValue();
            _effect.Duration = (int)DurationInput?.GetSelectedValue();
            _effect.ExtraTargets = (int)(ExtraTargetsInput?.GetSelectedValue());
                
            _effect.EntityId = _uiService.GetIntInput(EntityInput);
        }


    }
}
