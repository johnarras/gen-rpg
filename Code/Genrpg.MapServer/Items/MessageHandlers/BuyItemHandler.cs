using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.ServerShared.Achievements;
using Genrpg.Shared.Achievements.Constants;

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
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
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
            VendorItem vendorItem = null;
            lock (unit.OnActionLock)
            {
                if (unit.NPCStatus == null || unit.NPCStatus.Items == null)
                {
                    pack.SendError(gs, obj, "This shopkeeper has no items");
                    return;
                }

                vendorItem = unit.NPCStatus.Items.FirstOrDefault(x => x.Item != null && x.Item.Id == message.ItemId);

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

                unit.NPCStatus.Items.Remove(vendorItem);
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
