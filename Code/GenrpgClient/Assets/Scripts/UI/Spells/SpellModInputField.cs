using Genrpg.Shared.Interfaces;
using Genrpg.Shared.SpellCrafting.Entities;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;

namespace Assets.Scripts.UI.Spells
{
    public class SpellModInputField : BaseBehaviour
    {
        public GInputField InputField;
        public GText ModifierName;
        public GText InfoText;
        public GText InputText;
                
        private long _spellModifierId;
        private ISpellModifierHelper _helper = null;
        SpellModifier _modifier = null;


        private ISharedSpellCraftService _craftingService = null;
        public void Init(long spellModifierId)
        {
            _gs.loc.Resolve(this);
            _spellModifierId = spellModifierId;

            _helper = _craftingService.GetSpellModifierHelper(spellModifierId);
            _modifier = _gs.data.GetGameData<SpellModifierSettings>(_gs.ch).GetSpellModifier(spellModifierId);

            UIHelper.SetText(ModifierName, _modifier.Name);

            UIHelper.SetText(InfoText, _helper.GetInfoText(_gs, _gs.ch));

            UIHelper.SetText(InputText, _modifier.DefaultValue.ToString());

        }
    }
}
