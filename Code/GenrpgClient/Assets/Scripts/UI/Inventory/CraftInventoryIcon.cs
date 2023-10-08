using UnityEngine.EventSystems;
using Genrpg.Shared.Inventory.Entities;
using System.Threading;

public class CraftInventoryIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public GText InfoText;

    long currQuantity = 0;

    public void AddToQuantity(long amount)
    {
        currQuantity += amount;
        if (currQuantity < 0)
        {
            currQuantity = 0;
        }

        UIHelper.SetText(QuantityText, currQuantity.ToString());
    }
    
    public long GetQuantity()
    {
        return currQuantity;
    }

    public override void Init(InitItemIconData data, CancellationToken token)
    {
        base.Init(data, token);
        if (data == null || data.Data == null)
        {
            return;
        }

        _initData = data;

        InitItemIconData idata = new InitItemIconData()
        {
            Data = data.Data,
            Flags = data.Flags,
            handler = data.handler,
            Screen = data.Screen,
        };

        UIHelper.SetText(InfoText, ItemUtils.GetBasicInfo(_gs, _gs.ch, data.Data));

        currQuantity = idata.Data.Quantity;
    }
}

