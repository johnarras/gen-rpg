
using System.Globalization;
using GEntity = UnityEngine.GameObject;

public class MoneySegment : BaseBehaviour
{
    public GEntity Parent;
    public GText QuantityText;
    public GImage Icon;

    public GEntity GetParent()
    {
        return Parent;
    }

    public override void Initialize(UnityGameState gs)
    {
        base.Initialize(gs);
        if (!string.IsNullOrEmpty(_txt))
        {
            SetQuantityText(_txt);
        }
    }

    private string _txt = null;
    public void SetQuantityText(string txt)
    {
        _txt = txt;
        _uiService?.SetText(QuantityText, txt);
    }
}
