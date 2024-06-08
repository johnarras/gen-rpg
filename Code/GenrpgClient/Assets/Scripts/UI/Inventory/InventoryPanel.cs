using System.Collections.Generic;
using System.Linq;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Inventory.PlayerData;
using System.Threading;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Units.Entities;

public class InventoryPanel : BaseBehaviour
{
    
    public GEntity _iconParent;

    private int _category = 0;
    protected ItemIconScreen _screen = null;
    protected string _prefabName = "";
    private CancellationToken _token;
    private Unit _unit;
    public void Init(int categories, ItemIconScreen screen, Unit unit, string prefabName, CancellationToken token)
    {
        _unit = unit;
        _token = token;
        _screen = screen;
        _category = categories;
        _prefabName = prefabName;

        GEntityUtils.DestroyAllChildren(_iconParent);

        InventoryData inventory = _unit.Get<InventoryData>();

        List<Item> inventoryItems = inventory.GetAllInventory();

        List<Item> finalInventory = new List<Item>();

        foreach (Item item in inventoryItems)
        {
            ItemType itype = _gameData.Get<ItemTypeSettings>(_unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                continue;
            }

            if (itype.EquipSlotId > 0 || FlagUtils.IsSet(itype.Flags,ItemFlags.NoStack))
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
            IconPrefabName = _prefabName,
        };
        IconHelper.InitItemIcon(idata, _iconParent, _assetService, token);

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