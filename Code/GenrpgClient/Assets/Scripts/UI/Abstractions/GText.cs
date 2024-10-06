
using Genrpg.Shared.UI.Interfaces;

public class GText : TMPro.TextMeshProUGUI, IText
{
    public UnityEngine.Color Color { get { return color; } set {  color = value; } }
}
