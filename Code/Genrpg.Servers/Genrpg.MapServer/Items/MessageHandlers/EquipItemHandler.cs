using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class EquipItemHandler : BaseCharacterServerMapMessageHandler<EquipItem>
    {

        private IInventoryService _inventoryService = null;
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, EquipItem message)
        {
            if (!_inventoryService.EquipItem(ch, message.ItemId, message.EquipSlot))
            {
                pack.SendError(ch, "You can't equip that there");
                return;
            }
        }
    }
}
