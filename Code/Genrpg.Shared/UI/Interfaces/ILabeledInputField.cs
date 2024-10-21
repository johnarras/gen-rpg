using Genrpg.Shared.MVC.Interfaces;

namespace Genrpg.Shared.UI.Interfaces
{
    public interface ILabeledInputField : IViewElement
    {
        void SetLabel(string text);
        void SetPlaceholder(string text);
        void SetInputText(string text);
        string GetInputText();
    }
}
