
using System;
using Genrpg.Shared.UI.Interfaces;

namespace Assets.Scripts.UI.Core
{
    public class LabeledInputField : BaseBehaviour, ILabeledInputField
    {
        public GText Label;
        public GInputField Input;
        public GText Placeholder;
        public GText Text;

        public string GetInputText()
        {
            return Text.text;
        }

        public void Init(long spellModifierId, Action onValueChangedAction)
        {
        }

        public void SetLabel(string text)
        {
            _uiService.SetText(Label, text);
        }

        public void SetPlaceholder(string text)
        {
            _uiService.SetText(Placeholder, text);
        }

        public void SetInputText(string text)
        {
            _uiService.SetText(Text, text);
        }
    }
}
