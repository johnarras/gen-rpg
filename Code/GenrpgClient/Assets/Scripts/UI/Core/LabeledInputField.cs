using Genrpg.Shared.Interfaces;
using Genrpg.Shared.SpellCrafting.Settings;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.SpellCrafting.SpellModifierHelpers;
using System;

namespace Assets.Scripts.UI.Core
{
    public class LabeledInputField : BaseBehaviour
    {
        public GText Label;
        public GInputField Input;
        public GText Placeholder;
                
        public void Init(long spellModifierId, Action onValueChangedAction)
        {
            _gs.loc.Resolve(this);
        }
    }
}
