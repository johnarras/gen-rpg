using Amazon.Runtime.Internal.Util;
using AutoMapper.Internal.Mappers;
using Azure.ResourceManager.ServiceBus;
using Genrpg.MapServer.Items.Services;
using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Maps;
using Genrpg.MapServer.Trades.Services;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.MapObjectAddons;
using Genrpg.Shared.Vendors.Settings;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Vendors.Services
{
    public interface IVendorService : IInitializable
    {
        void UpdateItems(GameState gs, MapObject mapObject);
        void BuyItem(GameState gs, MapObject obj, BuyItem buyItem);
        void SellItem(GameState gs, MapObject obj, SellItem sellItem);
    }


    public class VendorService : IVendorService
    {
        private IInventoryService _inventoryService = null;
        private ICurrencyService _currencyService = null;
        private IAchievementService _achievementService = null;
        private ITradeService _tradeService = null;
        private IMapObjectManager _objectManager;
        private IMapMessageService _mapMessageService;
        public async Task Initialize(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IItemGenService _itemGenService = null;
        protected IRepositoryService _repoService = null;
        private IGameData _gameData;

        public void UpdateItems(GameState gs, MapObject mapObject)
        {

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();

            if (addon == null)
            {
                return;
            }

            double refreshMinutes = _gameData.Get<VendorSettings>(mapObject).VendorRefreshMinutes;

            if (refreshMinutes <= 0)
            {
                return;
            }

            if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
            {
                return;
            }

            int currItemCount = MathUtils.IntRange(addon.ItemCount, addon.ItemCount * 2, gs.rand);
            long level = mapObject.Level;
            Zone zone = gs.map.Get<Zone>(mapObject.ZoneId);

            if (zone != null)
            {
                level = zone.Level;
            }

            List<VendorItem> newItems = new List<VendorItem>();

            for (int i = 0; i < currItemCount; i++)
            {
                ItemGenData igd = new ItemGenData()
                {
                    Level = level,
                    Quantity = 1,
                };

                Item item = _itemGenService.Generate(gs, igd);

                if (item != null)
                {
                    newItems.Add(new VendorItem() { Item = item, Quantity = 1 });
                }
            }
            lock (mapObject.OnActionLock)
            {
                if (addon.LastRefreshTime >= DateTime.UtcNow.AddMinutes(-refreshMinutes))
                {
                    return;
                }

                addon.Items = newItems;
                addon.LastRefreshTime = DateTime.UtcNow;

                if (mapObject.Spawn is MapSpawn mapSpawn)
                {
                    mapSpawn = SerializationUtils.FastMakeCopy(mapSpawn);
                    mapSpawn.AddonString = SerializationUtils.Serialize(mapSpawn.Addons);
                    mapSpawn.Addons = null;
                    _repoService.QueueSave(mapSpawn);
                }
            }
        }

        public void BuyItem(GameState gs, MapObject obj, BuyItem buyItem)
        {
            _tradeService.SafeModifyObject(obj, delegate { BuyItemInternal(gs, obj, buyItem); });
        }

        private void BuyItemInternal (GameState gs, MapObject obj, BuyItem buyItem)
        { 
            if (!_objectManager.GetObject(buyItem.UnitId, out MapObject vendor))
            {
                obj.AddMessage(new ErrorMessage("Shopkeeper doesn't exist."));
                return;
            }

            if (!(obj is Character ch))
            {
                obj.AddMessage(new ErrorMessage("Only players can buy items."));
                return;
            }

            CurrencyData cdata = ch.Get<CurrencyData>();
            InventoryData idata = ch.Get<InventoryData>();

            long playerMoney = cdata.GetQuantity(CurrencyTypes.Money);
            long itemPrice = 0;

            VendorAddon addon = vendor.GetAddon<VendorAddon>();

            if (addon == null || addon.Items == null)
            {
                obj.AddMessage(new ErrorMessage("Shopkeeper doesn't exist."));
                return;
            }

            VendorItem vendorItem = addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == buyItem.ItemId);

            if (vendorItem == null || vendorItem.Item == null)
            {
                obj.AddMessage(new ErrorMessage("Shopkeeper doesn't exist."));
                return;
            }

            if (vendorItem.Quantity > 0)
            {
                lock (vendor.OnActionLock)
                {
                    addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == buyItem.ItemId);

                    if (vendorItem == null || vendorItem.Item == null)
                    {
                        obj.AddMessage(new ErrorMessage("Shopkeeper doesn't exist."));
                        return;
                    }

                    itemPrice = ItemUtils.GetBuyFromVendorPrice(_gameData, ch, vendorItem.Item);

                    if (itemPrice > playerMoney)
                    {
                        obj.AddMessage(new ErrorMessage("Shopkeeper doesn't exist."));
                        return;
                    }
                    addon.Items.Remove(vendorItem);
                }
            }

            if (vendorItem != null)
            {
                _currencyService.Add(ch, CurrencyTypes.Money, -itemPrice);
                _inventoryService.AddItem(ch, vendorItem.Item, true);
                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.ItemsBought, 1);
            }
        }

        public void SellItem(GameState gs, MapObject obj, SellItem sellItem)
        {
            _tradeService.SafeModifyObject(obj, delegate { SellItemInternal(gs, obj, sellItem); });
        }

        private void SellItemInternal(GameState gs, MapObject obj, SellItem sellItem)
        { 
            if (!_objectManager.GetObject(sellItem.UnitId, out MapObject mapObject))
            {
                obj.AddMessage(new ErrorMessage("That vendor doesn't exist."));
                return;
            }

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();


            if (addon == null)
            {
                obj.AddMessage(new ErrorMessage("This isn't a vendor."));
                return;
            }


            if (!(obj is Character ch))
            {
                obj.AddMessage(new ErrorMessage("Only players can sell items."));
                return;
            }

            InventoryData idata = ch.Get<InventoryData>();
            CurrencyData cdata = ch.Get<CurrencyData>();

            Item item = idata.GetItem(sellItem.ItemId);

            if (item == null)
            {
                obj.AddMessage(new ErrorMessage("You don't have that item."));
                return;
            }

            long money = ItemUtils.GetSellToVendorPrice(_gameData, ch, item);

            _inventoryService.RemoveItem(ch, sellItem.ItemId, true);
            _achievementService.UpdateAchievement(gs, ch, AchievementTypes.ItemsSold, item.Quantity);
            _currencyService.Add(ch, CurrencyTypes.Money, money);
            _achievementService.UpdateAchievement(gs, ch, AchievementTypes.VendorMoney, money);
        }
    }
}
