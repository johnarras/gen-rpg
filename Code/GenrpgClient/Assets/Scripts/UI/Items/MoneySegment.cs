
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

    public void SetQuantityText(string txt)
    {
        UIHelper.SetText(QuantityText, txt);
    }
}
