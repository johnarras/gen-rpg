using Genrpg.Shared.Interfaces;
using Genrpg.Shared.SpellCrafting.Settings;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using System;

namespace Assets.Scripts.UI.Spells
{
    public class SpellModInputField : BaseBehaviour
    {
        public GDropdown Dropdown;
        public GText ModifierName;
        public GText InfoText;
                
        private long _spellModifierId;
        private ISpellModifierHelper _helper = null;
        SpellModifier _modifier = null;


        private ISharedSpellCraftService _craftingService = null;
        public void Init(long spellModifierId, Action onValueChangedAction)
        {
            _spellModifierId = spellModifierId;

            _helper = _craftingService.GetSpellModifierHelper(spellModifierId);
            _modifier = _gameData.Get<SpellModifierSettings>(_gs.ch).Get(spellModifierId);

            _uIInitializable.SetText(ModifierName, _modifier.Name);

            _uIInitializable.SetText(InfoText, "");// _helper.GetInfoText(_gs.ch));

            Dropdown?.Init(_helper.GetValidValues(_gs.ch), onValueChangedAction);

            
        }

        public double GetSelectedValue()
        {
            if (Dropdown == null)
            {
                return 0;
            }

            if (double.TryParse(Dropdown.captionText.text, out double value))
            {
                return value;
            }

            return 0;
        }

        public void SetSelectedValue(double value)
        {
            Dropdown?.SetFromValue(value);
        }


    }
}
