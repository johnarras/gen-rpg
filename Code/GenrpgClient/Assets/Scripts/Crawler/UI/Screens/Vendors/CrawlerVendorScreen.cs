
using Genrpg.Shared.Inventory.Services;
using System.Linq;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Vendors.Settings;
using System;
using System.Collections.Generic;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Services;
using System.Threading.Tasks;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.States.Constants;

public class CrawlerVendorScreen : ItemIconScreen
{

    protected ICrawlerService _crawlerService;
    protected IInventoryService _inventoryService;
    protected ILootGenService _lootGenService;
    private ICrawlerWorldService _crawlerWorldService;
    private IIconService _iconService;
    
    public const string VendorIconName = "VendorItemIcon";

    public InventoryPanel PlayerItems;
    public GameObject VendorItems;


    public GText PartyGoldText;

    PartyData _party;
    PartyMember _member;
    protected override async Task OnStartOpen(object data, CancellationToken token)
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


    private async void ShowVendorItems()
    {
        _gameObjectService.DestroyAllChildren(VendorItems);

        if (VendorItems == null)
        {
            return;        
        }

        VendorSettings settings = _gameData.Get<VendorSettings>(null);

        if (_party.VendorItems.Count < 1 || (_party.LastVendorRefresh < DateTime.UtcNow.AddMinutes(-settings.VendorRefreshMinutes)))
        {
            _party.VendorItems = new List<Item>();

            _party.LastVendorRefresh = DateTime.UtcNow;


            int quantity = MathUtils.IntRange(4, 10, _rand);

            for (int i = 0; i < quantity; i++)
            {

                ItemGenData lootGenData = new ItemGenData()
                {
                    Level = await _crawlerWorldService.GetMapLevelAtParty(await _crawlerWorldService.GetWorld(_party.WorldId), _party)
                };

                _party.VendorItems.Add(_lootGenService.GenerateItem(lootGenData));
            }
        }

        foreach (Item item in _party.VendorItems)
        {
            InitItemIconData idata = new InitItemIconData()
            {
                Data = item,
                Flags = ItemIconFlags.IsVendorItem | ItemIconFlags.ShowTooltipOnRight,
                IconPrefabName = VendorIconName,
                Screen = this,
            };
            _iconService.InitItemIcon(idata, VendorItems, _assetService, _token);
        }

        _uiService.SetText(PartyGoldText, StrUtils.PrintCommaValue(_party.Gold));
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


    private void BuyItem(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item vendorItem = _party.VendorItems.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);
        if (vendorItem == null)
        {
            _dispatcher.Dispatch(new ShowFloatingText("That item isn't for sale!", EFloatingTextArt.Error));
            return;
        }

        if (vendorItem.BuyCost > _party.Gold)
        {
            _dispatcher.Dispatch(new ShowFloatingText("You need more gold to buy this!", EFloatingTextArt.Error));
            return;
        }

        _party.Gold -= vendorItem.BuyCost;

        _party.VendorItems.Remove(icon.GetDataItem());
        _inventoryService.AddItem(_member, icon.GetDataItem(), true);
        ShowVendorItems();
        InitPanel();
    }

    private void SellItem(ItemIcon icon)
    {
        if (icon == null || icon.GetDataItem() == null)
        {
            return;
        }

        Item item = _party.Inventory.FirstOrDefault(x => x.Id == icon.GetDataItem().Id);

        if (item == null)
        {
            _dispatcher.Dispatch(new ShowFloatingText("You don't have that item!", EFloatingTextArt.Error));
            return;
        }

        _party.Gold += (long)(item.SellValue);

        _inventoryService.RemoveItem(_member, icon.GetDataItem().Id, false);
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

