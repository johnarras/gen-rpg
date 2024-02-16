using System;
using System.Collections.Generic;
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
using Genrpg.Shared.Vendors.MapObjectAddons;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class SellItemHandler : BaseServerMapMessageHandler<SellItem>
    {

        private IInventoryService _inventoryService = null;
        private ICurrencyService _currencyService = null;
        private IAchievementService _achievementService = null;

        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SellItem message)
        {
            if (!_objectManager.GetObject(message.UnitId, out MapObject mapObject))
            {
                pack.SendError(gs, obj, "That vendor doesn't exist");
                return;
            }

            VendorAddon addon = mapObject.GetAddon<VendorAddon>();


            if (addon == null)
            {
                pack.SendError(gs, obj, "This isn't a vendor");
                return;
            }


            if (!(obj is Character ch))
            {
                pack.SendError(gs, obj, "Only players can sell items");
                return;
            }

            InventoryData idata = ch.Get<InventoryData>();
            CurrencyData cdata = ch.Get<CurrencyData>();

            Item item = idata.GetItem(message.ItemId);

            if (item == null)
            {
                pack.SendError(gs, obj, "You don't have that item");
                return;
            }

            long money = ItemUtils.GetSellToVendorPrice(gs, ch, item);

            _inventoryService.RemoveItem(gs, ch, message.ItemId,true);
            _achievementService.UpdateAchievement(gs, ch, AchievementTypes.ItemsSold, item.Quantity);
            _currencyService.Add(gs, ch, CurrencyTypes.Money, money);
            _achievementService.UpdateAchievement(gs, ch, AchievementTypes.VendorMoney, money);
        }
    }
}
