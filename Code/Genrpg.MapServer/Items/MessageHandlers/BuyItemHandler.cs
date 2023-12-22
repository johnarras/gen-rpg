using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Utils;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Vendors.MapObjectAddons;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class BuyItemHandler : BaseServerMapMessageHandler<BuyItem>
    {
        private IInventoryService _inventoryService = null;
        private ICurrencyService _currencyService = null;
        private IAchievementService _achievementService = null;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, BuyItem message)
        {
            if (!_objectManager.GetObject(message.UnitId, out MapObject vendor))
            {
                pack.SendError(gs, obj, "This shopkeeper doesn't exist");
                return;
            }

            if (!(obj is Character ch))
            {
                pack.SendError(gs, obj, "Only players can buy items");
                return;
            }

            CurrencyData cdata = ch.Get<CurrencyData>();
            InventoryData idata = ch.Get<InventoryData>();

            long playerMoney = cdata.GetQuantity(CurrencyTypes.Money);
            long itemPrice = 0;

            VendorAddon addon = vendor.GetAddon<VendorAddon>();

            if (addon == null || addon.Items == null)
            {
                pack.SendError(gs, obj, "This shopkeeper has no items");
                return;
            }

            VendorItem vendorItem = addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == message.ItemId);

            if (vendorItem == null || vendorItem.Item == null)
            {
                pack.SendError(gs, obj, "That item doesn't exist");
                return;
            }

            if (vendorItem.Quantity > 0)
            {
                lock (vendor.OnActionLock)
                {
                    addon.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == message.ItemId);

                    if (vendorItem == null || vendorItem.Item == null)
                    {
                        pack.SendError(gs, obj, "That item doesn't exist");
                        return;
                    }

                    itemPrice = ItemUtils.GetBuyFromVendorPrice(gs, ch, vendorItem.Item);

                    if (itemPrice > playerMoney)
                    {
                        pack.SendError(gs, obj, "You don't have enough money to purchase this");
                        return;
                    }
                    addon.Items.Remove(vendorItem);
                }
            }

            if (vendorItem != null)
            {
                _currencyService.Add(gs, ch, CurrencyTypes.Money, -itemPrice);
                _inventoryService.AddItem(gs, ch, vendorItem.Item, true);
                _achievementService.UpdateAchievement(gs, ch, AchievementTypes.ItemsBought, 1);
            }

        }
    }
}
