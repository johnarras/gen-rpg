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
using Genrpg.Shared.Inventory.Entities;
using System.Threading;

public class CraftInventoryIcon : ItemIcon, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField]
    private Text _info;

    long currQuantity = 0;

    public void AddToQuantity(long amount)
    {
        currQuantity += amount;
        if (currQuantity < 0)
        {
            currQuantity = 0;
        }

        UIHelper.SetText(_quantityText, currQuantity.ToString());
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

        UIHelper.SetText(_info, ItemUtils.GetBasicInfo(_gs, data.Data));

        currQuantity = idata.Data.Quantity;
    }
}

