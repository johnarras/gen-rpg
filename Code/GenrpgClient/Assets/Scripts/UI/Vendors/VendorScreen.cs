using Cysharp.Threading.Tasks;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.NPCs.Messages;
using Genrpg.Shared.Units.Entities;
using System.Linq;
using System.Threading;
using GEntity = UnityEngine.GameObject;

public class VendorScreen : ItemIconScreen
{
    protected IInventoryService _inventoryService;
    public const string VendorIconName = "VendorItemIcon";

    public InventoryPanel PlayerItems;
    public GEntity VendorItems;
    public GButton CloseButton;

    NPCStatus _status = null;
    NPCType _type = null;


    Unit _unit = null;

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);
        UIHelper.SetButton(CloseButton, GetAnalyticsName(), StartClose);
        _gs.AddEvent<OnGetNPCStatus>(this, OnGetNPCStatusHandler);
        _gs.AddEvent<OnAddItem>(this, OnAddItemHandler);
        _gs.AddEvent<OnRemoveItem>(this, OnRemoveItemHandler);
        _unit = data as Unit;

        if (_unit == null)
        {
            StartClose();
            return;
        }

        _type = _unit.NPCType;

        if (_type == null)
        {
            StartClose();
            return;
        }

        InitPanel();

        _networkService.SendMapMessage(new GetNPCStatus() { UnitId = _unit.Id });

    }

    private OnGetNPCStatus OnGetNPCStatusHandler(UnityGameState gs, OnGetNPCStatus status)
    {
        ShowVendorItems(status.Status);

        return null;
    }

    private void InitPanel()
    {
        PlayerItems.Init(InventoryGroup.All, this, null, _token);
    }

    private OnAddItem OnAddItemHandler (UnityGameState gs, OnAddItem addItem)
    {
        if (_status != null)
        {
            VendorItem vitem = _status.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == addItem.ItemId);
            if (vitem != null)
            {
                _status.Items.Remove(vitem);
                ShowVendorItems(_status);

                _inventoryService.AddItem(gs, gs.ch, vitem.Item, true);
                InitPanel();
            }
        }
        return null;
    }


    private OnRemoveItem OnRemoveItemHandler(UnityGameState gs, OnRemoveItem removeItem)
    {
        if (removeItem != null && removeItem.ItemId != null)
        {
            _inventoryService.RemoveItem(gs, gs.ch, removeItem.ItemId,false);
            InitPanel();
        }
        return null;
    }

    private void ShowVendorItems(NPCStatus status)
    {
        _status = status;
        if (VendorItems == null || _status == null)
        {
            return;
        }

        GEntityUtils.DestroyAllChildren(VendorItems);
            
        if (_status == null || _status.Items == null)
        {
            return;
        }

        foreach (VendorItem item in _status.Items)
        {
            InitItemIconData idata = new InitItemIconData()
            {
                Data = item.Item,
                Flags = ItemIconFlags.IsVendorItem | ItemIconFlags.ShowTooltipOnRight,
                iconPrefabName = VendorIconName,
                Screen = this,
            };
            IconHelper.InitItemIcon(_gs, idata, VendorItems,_assetService, _token);
        }



    }


    // Blank
    public override void OnLeftClickIcon(UnityGameState gs, ItemIcon icon) { }

  


    // Equip or Unequip item.
    public override void OnRightClickIcon(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }


        if (icon.HasFlag(ItemIconFlags.IsVendorItem))
        {
            BuyItem(gs, icon);
        }
        else
        {
            SellItem(gs, icon);
        }
    }


    private void BuyItem (UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        _networkService.SendMapMessage(new BuyItem() { ItemId = icon.GetDataItem().Id, UnitId = _unit.Id });

    }

    private void SellItem (UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        _networkService.SendMapMessage(new SellItem() { ItemId = icon.GetDataItem().Id, UnitId = _unit.Id });
    }
}

