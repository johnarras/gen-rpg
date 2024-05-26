using UnityEngine.EventSystems;
using Genrpg.Shared.Inventory.PlayerData;
using System.Threading;
using Genrpg.Shared.Inventory.Utils;

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

        _uIInitializable.SetText(QuantityText, currQuantity.ToString());
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
            Handler = data.Handler,
            Screen = data.Screen,
        };

        _uIInitializable.SetText(InfoText, ItemUtils.GetBasicInfo(_gameData, _gs.ch, data.Data));

        currQuantity = idata.Data.Quantity;
    }
}

