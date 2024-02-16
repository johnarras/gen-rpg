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
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Assets.Scripts.Crawler.Services;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Vendors.Settings;
using System;
using System.Collections.Generic;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Utils;
using Assets.Scripts.UI.Crawler.States;

public class CrawlerVendorScreen : ItemIconScreen
{

    protected ICrawlerService _crawlerService;
    protected IInventoryService _inventoryService;
    protected ILootGenService _lootGenService;
    public const string VendorIconName = "VendorItemIcon";

    public InventoryPanel PlayerItems;
    public GEntity VendorItems;


    public GText PartyGoldText;

    PartyData _party;
    PartyMember _member;
    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await base.OnStartOpen(data, token);

        _party = _crawlerService.GetParty();
        _member = _party.GetActiveParty().First();
        InitPanel();
        ShowVendorItems();

    }

    private void InitPanel()
    {

        InventoryData inventoryData = _member.Get<InventoryData>();

        inventoryData.SetInvenEquip(_party.Inventory, _member.Equipment);

        PlayerItems.Init(InventoryGroup.All, this, _member, null, _token);
    }


    private void ShowVendorItems()
    {
        GEntityUtils.DestroyAllChildren(VendorItems);

        if (VendorItems == null)
        {
            return;        
        }

        VendorSettings settings = _gs.data.Get<VendorSettings>(null);

        if (_party.VendorItems.Count < 1 || (_party.LastVendorRefresh < DateTime.UtcNow.AddMinutes(-settings.VendorRefreshMinutes)))
        {
            _party.VendorItems = new List<Item>();

            _party.LastVendorRefresh = DateTime.UtcNow;


            int quantity = MathUtils.IntRange(4, 10, _gs.rand);

            for (int i = 0; i < quantity; i++)
            {

                LootGenData lootGenData = new LootGenData()
                {
                    Level = _party.GetWorldLevel(),
                };

                _party.VendorItems.Add(_lootGenService.GenerateItem(_gs, lootGenData));
            }
        }

        foreach (Item item in _party.VendorItems)
        {
            InitItemIconData idata = new InitItemIconData()
            {
                Data = item,
                Flags = ItemIconFlags.IsVendorItem | ItemIconFlags.ShowTooltipOnRight,
                iconPrefabName = VendorIconName,
                Screen = this,
            };
            IconHelper.InitItemIcon(_gs, idata, VendorItems, _assetService, _token);
        }

        _uiService.SetText(PartyGoldText, StrUtils.PrintCommaValue(_party.Gold));
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


    private void BuyItem(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item vendorItem = _party.VendorItems.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);
        if (vendorItem == null)
        {
            FloatingTextScreen.Instance.ShowError("That item isn't for sale!");
            return;
        }

        if (vendorItem.Cost > _party.Gold)
        {
            FloatingTextScreen.Instance.ShowError("You need more gold to buy this!");
            return;
        }

        _party.Gold -= vendorItem.Cost;

        _party.VendorItems.Remove(icon.GetDataItem());
        _inventoryService.AddItem(gs, _member, icon.GetDataItem(), true);
        ShowVendorItems();
        InitPanel();
    }

    private void SellItem(UnityGameState gs, ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item item = _party.Inventory.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);

        if (item == null)
        {
            FloatingTextScreen.Instance.ShowError("You don't have that item!");
            return;
        }

        _party.Gold += item.Cost;

        _inventoryService.RemoveItem(gs, _member, icon.GetDataItem().Id, false);
        _party.VendorBuyback.Add(item);

        while (_party.VendorBuyback.Count > 10)
        {
            _party.VendorBuyback.RemoveAt(0);
        }
        ShowVendorItems();
        InitPanel();
    }

    protected override void OnStartClose()
    {
        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, _token);
        base.OnStartClose();
    }
}

