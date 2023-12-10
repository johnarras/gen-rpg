using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;

namespace Assets.Scripts.UI.Spells
{
    public class SpellEffectEdit : BaseBehaviour
    {
        public GInputField SkillInput;
        public SpellModInputField ScaleInput;
        public SpellModInputField RadiusInput;
        public SpellModInputField DurationInput;
        public SpellModInputField ExtraTargetsInput;
        public GInputField EntityInput;


        private SpellEffect _effect;
        private Spell _spell;
        private SpellbookScreen _screen;

        public void Init(SpellEffect effect, Spell spell, SpellbookScreen screen)
        {
            

            _effect = effect;
            _spell = spell;
            _screen = screen;

            CopyFromEffectToUI();

        }

        public void CopyFromEffectToUI()
        {
            UIHelper.SetInputText(SkillInput, _effect.SkillTypeId);
            UIHelper.SetInputText(ScaleInput.InputField, _effect.Scale);
            UIHelper.SetInputText(RadiusInput.InputField, _effect.Radius);
            UIHelper.SetInputText(DurationInput.InputField, _effect.Duration);
            UIHelper.SetInputText(ExtraTargetsInput.InputField, _effect.ExtraTargets);
            UIHelper.SetInputText(EntityInput, _effect.EntityId);
        }

        public void CopyFromUIToEffect()
        {
            _effect.SkillTypeId = UIHelper.GetIntInput(SkillInput);
            _effect.Scale = UIHelper.GetIntInput(ScaleInput.InputField);
            _effect.Radius = UIHelper.GetIntInput(RadiusInput.InputField);
            _effect.Duration = UIHelper.GetIntInput(DurationInput.InputField);
            _effect.ExtraTargets = UIHelper.GetIntInput(ExtraTargetsInput.InputField);
            _effect.EntityId = UIHelper.GetIntInput(EntityInput);
        }


    }
}
