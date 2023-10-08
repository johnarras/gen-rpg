
using UnityEngine.EventSystems;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Inventory.Entities;
using System.Threading;

public class VendorItemIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
{
    
    public GText ItemName;
    public GText ItemInfo;
    public MoneyDisplay _moneyDisplay;

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
     

        UIHelper.SetText(ItemName, ItemUtils.GetName(_gs, _gs.ch, data.Data));
        UIHelper.SetText(ItemInfo, ItemUtils.GetBasicInfo(_gs, _gs.ch, data.Data));

        _price = (isVendorItem ? ItemUtils.GetBuyFromVendorPrice(_gs, _gs.ch, data.Data)
            : ItemUtils.GetSellToVendorPrice(_gs, _gs.ch, data.Data));

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
