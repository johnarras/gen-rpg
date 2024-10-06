
using System.Globalization;
using UnityEngine;

public class MoneySegment : BaseBehaviour
{
    public GameObject Parent;
    public GText QuantityText;
    public GImage Icon;

    public GameObject GetParent()
    {
        return Parent;
    }

    public override void Init()
    {
        base.Init();
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
