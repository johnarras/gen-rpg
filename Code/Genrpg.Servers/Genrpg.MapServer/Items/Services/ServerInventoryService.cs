using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.Services
{
    public class ServerInventoryService : InventoryService
    {
        protected IRepositoryService _repoService = null;
        private IMapMessageService _messageService = null;
        private ITradeService _tradeService = null;

        public override bool AddItem(Unit unit, Item item, bool forceAdd)
        {
            return base.AddItem(unit, item, forceAdd);
        }

        public override bool UnequipItem(Unit unit, string itemId, bool calcStatsNow = true)
        {
            return _tradeService.SafeModifyObject(unit, delegate { return base.UnequipItem(unit, itemId, calcStatsNow); }, false);

        }

        public override Item RemoveItemQuantity(Unit unit, string itemId, int quantity)
        {
            return _tradeService.SafeModifyObject(unit, delegate
            {
                return base.RemoveItemQuantity(unit, itemId, quantity);
            }, null);
        }



        public override Item RemoveItem(Unit unit, string itemId, bool destroyItem)
        {
            return _tradeService.SafeModifyObject(unit, delegate
            {
                return base.RemoveItem(unit, itemId, destroyItem);
            }, null);
        }

        public override bool EquipItem(Unit unit, string itemId, long equipSlotId, bool calcStatsNow = true)
        {
            return _tradeService.SafeModifyObject(unit, delegate
            {
                return base.EquipItem(unit, itemId, equipSlotId, calcStatsNow);
            }, false);
        }

        protected override void AddMessage(Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessage(unit, message);
            if (dataUpdateType == EDataUpdateTypes.Save)
            {
                _repoService.QueueSave(item);
            }
            else if (dataUpdateType == EDataUpdateTypes.Delete)
            {
                _repoService.QueueDelete(item);
            }
        }

        protected override void AddMessageNear(Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessageNear(unit, message);
            _repoService.QueueSave(item);
            if (dataUpdateType == EDataUpdateTypes.Save)
            {
                _repoService.QueueSave(item);
            }
            else if (dataUpdateType == EDataUpdateTypes.Delete)
            {
                _repoService.QueueDelete(item);
            }
        }
    }
}
