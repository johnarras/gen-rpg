using Cysharp.Threading.Tasks;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Units.Entities;
using System.Linq;
using System.Threading;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.MapObjects.MapObjectAddons.Constants;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.Vendors.WorldData;

public class VendorScreen : ItemIconScreen
{
    protected IInventoryService _inventoryService;
    public const string VendorIconName = "VendorItemIcon";

    public InventoryPanel PlayerItems;
    public GEntity VendorItems;


    Unit _unit = null;

    VendorAddon _addon = null;
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        _dispatcher.AddEvent<OnGetMapObjectStatus>(this, OnGetNPCStatusHandler);
        _dispatcher.AddEvent<OnAddItem>(this, OnAddItemHandler);
        _dispatcher.AddEvent<OnRemoveItem>(this, OnRemoveItemHandler);
        _unit = data as Unit;

        if (_unit == null || !_unit.HasAddon(MapObjectAddonTypes.Vendor))
        {
            StartClose();
            return;
        }

        InitPanel();

        _networkService.SendMapMessage(new GetMapObjectStatus() { ObjId =_unit.Id });

    }

    private void OnGetNPCStatusHandler(OnGetMapObjectStatus status)
    {
        VendorAddon addon = (VendorAddon)status.Addons.FirstOrDefault(x=>x.GetAddonType() == MapObjectAddonTypes.Vendor);


        ShowVendorItems(addon);

        return;
    }

    private void InitPanel()
    {
        PlayerItems.Init(InventoryGroup.All, this, _gs.ch, null, _token);
    }

    private void OnAddItemHandler (OnAddItem addItem)
    {
        if (_addon == null)
        {
            return;
        }

        VendorItem vitem = _addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == addItem.ItemId);
        if (vitem != null)
        {
            _addon.Items.Remove(vitem);
            ShowVendorItems(_addon);
            _inventoryService.AddItem(_gs.ch, vitem.Item, true);
            InitPanel();
        }
        return;
    }


    private void OnRemoveItemHandler(OnRemoveItem removeItem)
    {
        if (removeItem != null && removeItem.ItemId != null)
        {
            _inventoryService.RemoveItem(_gs.ch, removeItem.ItemId,false);
            InitPanel();
        }
        return;
    }

    private void ShowVendorItems(VendorAddon addon)
    {
        GEntityUtils.DestroyAllChildren(VendorItems);

        _addon = addon;
        if (VendorItems == null || addon == null || addon.Items == null)
        {
            return;
        }

        foreach (VendorItem item in addon.Items)
        {
            InitItemIconData idata = new InitItemIconData()
            {
                Data = item.Item,
                Flags = ItemIconFlags.IsVendorItem | ItemIconFlags.ShowTooltipOnRight,
                IconPrefabName = VendorIconName,
                Screen = this,
            };
            IconHelper.InitItemIcon(idata, VendorItems,_assetService, _token);
        }
    }

    // Blank
    public override void OnLeftClickIcon(ItemIcon icon) { }

  


    // Equip or Unequip item.
    public override void OnRightClickIcon(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }


        if (icon.HasFlag(ItemIconFlags.IsVendorItem))
        {
            BuyItem(icon);
        }
        else
        {
            SellItem(icon);
        }
    }


    private void BuyItem (ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        _networkService.SendMapMessage(new BuyItem() { ItemId = icon.GetDataItem().Id, UnitId = _unit.Id });

    }

    private void SellItem (ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        _networkService.SendMapMessage(new SellItem() { ItemId = icon.GetDataItem().Id, UnitId = _unit.Id });
    }
}

