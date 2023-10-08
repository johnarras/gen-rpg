using System;
using System.Collections.Generic;
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

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class SellItemHandler : BaseServerMapMessageHandler<SellItem>
    {

        private IInventoryService _inventoryService = null;
        private ICurrencyService _currencyService = null;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SellItem message)
        {
            if (!_objectManager.GetUnit(message.UnitId, out Unit unit))
            {
                pack.SendError(gs, obj, "That vendor doesn't exist");
                return;
            }


            if (unit.NPCTypeId < 1 || unit.NPCType == null || unit.NPCType.ItemCount < 1)
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
            _currencyService.Add(gs, ch, CurrencyType.Money, money);
        }
    }
}
