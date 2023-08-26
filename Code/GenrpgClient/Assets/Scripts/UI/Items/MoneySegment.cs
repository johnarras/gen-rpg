using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MoneySegment : BaseBehaviour
{
    [SerializeField]
    private GameObject _parent;
    [SerializeField]
    private Text _quantity;
    [SerializeField]
    private Image _icon;

    public GameObject GetParent()
    {
        return _parent;
    }

    public void SetQuantityText(string txt)
    {
        UIHelper.SetText(_quantity, txt);
    }
}
