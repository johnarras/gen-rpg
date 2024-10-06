
using UnityEngine.EventSystems;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Inventory.Services;

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
            Handler = data.Handler,
            Screen = data.Screen,
        };
     

        _uiService.SetText(ItemName, _sharedItemService.GetName(_gameData, _gs.ch, data.Data));
        _uiService.SetText(ItemInfo, _sharedItemService.GetBasicInfo(_gameData, _gs.ch, data.Data));

        _price = (isVendorItem ? data.Data.BuyCost : data.Data.SellValue);

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
