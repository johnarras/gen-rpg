using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Constants;
using Genrpg.Shared.DataStores.Entities;
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

namespace Genrpg.MapServer.Items
{
    public class ServerInventoryService : InventoryService
    {
        protected IRepositoryService _repoService = null;
        private IMapMessageService _messageService = null;
        public override async Task Initialize(GameState gs, CancellationToken token)
        {
            await base.Initialize(gs, token);
        }

        protected override void AddMessage(GameState gs, Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessage(unit, message);
            if (dataUpdateType == EDataUpdateTypes.Save)
            {
                item.SetDirty(true);
            }
            else if (dataUpdateType == EDataUpdateTypes.Delete)
            {
                _repoService.QueueDelete(item);
            }
        }

        protected override void AddMessageNear(GameState gs, Unit unit, InventoryData idata, Item item, IMapApiMessage message, EDataUpdateTypes dataUpdateType = EDataUpdateTypes.Save)
        {
            _messageService.SendMessageNear(unit, message);
            item.SetDirty(true);
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
