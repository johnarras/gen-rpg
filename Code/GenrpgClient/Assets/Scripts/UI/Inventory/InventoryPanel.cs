using System.Collections.Generic;
using System.Linq;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Inventory.Entities;
using System.Threading;

public class InventoryPanel : BaseBehaviour
{
    
    public GEntity _iconParent;

    private int _category = 0;
    protected ItemIconScreen _screen = null;
    protected string _prefabName = "";
    private CancellationToken _token;
    public void Init(int categories, ItemIconScreen screen, string prefabName, CancellationToken token)
    {
        _token = token;
        _screen = screen;
        _category = categories;
        _prefabName = prefabName;

        GEntityUtils.DestroyAllChildren(_iconParent);

        InventoryData inventory = _gs.ch.Get<InventoryData>();

        List<Item> inventoryItems = inventory.GetAllInventory();

        List<Item> finalInventory = new List<Item>();

        foreach (Item item in inventoryItems)
        {
            ItemType itype = _gs.data.GetGameData<ItemTypeSettings>(_gs.ch).GetItemType(item.ItemTypeId);
            if (itype == null)
            {
                continue;
            }

            if (itype.EquipSlotId > 0 || FlagUtils.IsSet(itype.Flags,ItemType.NoStack))
            {
                if (FlagUtils.IsSet(categories,InventoryGroup.Equipment))
                {
                    finalInventory.Add(item);
                }
            }
            else
            {
                if (FlagUtils.IsSet(categories,InventoryGroup.Reagents))
                {
                    finalInventory.Add(item);
                }
            }
        }



        foreach (Item item in finalInventory)
        {
            InitIcon(item, token);
        }

    }

    public void InitIcon(Item item, CancellationToken token)
    {
        InitItemIconData idata = new InitItemIconData()
        {
            Data = item,
            Screen = _screen,
            iconPrefabName = _prefabName,
        };
        IconHelper.InitItemIcon(_gs, idata, _iconParent, _assetService, token);

    }

    public void RemoveIcon(string itemId)
    {
        List<ItemIcon> allIcons = GEntityUtils.GetComponents<ItemIcon>(_iconParent);

        ItemIcon desiredIcon = allIcons.FirstOrDefault(x => x.GetDataItem() != null &&
        x.GetDataItem().Id == itemId);

        if (desiredIcon != null)
        {
            GEntityUtils.Destroy(desiredIcon.entity());
        }
    }

}