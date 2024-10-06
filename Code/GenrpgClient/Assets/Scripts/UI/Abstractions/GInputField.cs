
using Genrpg.Shared.UI.Interfaces;

public class GInputField : TMPro.TMP_InputField, IInputField
{
    public string Text { get { return text; } set { text = value; } }
}