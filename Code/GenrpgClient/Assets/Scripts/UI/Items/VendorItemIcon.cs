using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Core.Entities;

using Services;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Inventory.Entities;
using System.Threading;

public class VendorItemIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField]
    private Text _name;
    [SerializeField]
    private Text _info;

    [SerializeField]
    private MoneyDisplay _moneyDisplay;

    private long _price = 0;

    public long GetPrice()
    {
        return _price;
    }
    private bool isVendorItem = false;

    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null || data.Data == null)
        {
            return;
        }

        _initData = data;

        isVendorItem = (FlagUtils.IsSet(_initData.Flags, ItemIconFlags.IsVendorItem));

        InitItemIconData idata = new InitItemIconData()
        {
            Data = data.Data,
            Flags = data.Flags,
            handler = data.handler,
            Screen = data.Screen,
        };
     

        UIHelper.SetText(_name, ItemUtils.GetName(_gs, data.Data));
        UIHelper.SetText(_info, ItemUtils.GetBasicInfo(_gs, data.Data));

        _price = (isVendorItem ? ItemUtils.GetBuyFromVendorPrice(_gs, data.Data)
            : ItemUtils.GetSellToVendorPrice(_gs, data.Data));

        if (_moneyDisplay != null)
        {
            _moneyDisplay.SetMoney(_price);
        }
    }

    public override bool CanDrag()
    {
        return false;
    }





}
