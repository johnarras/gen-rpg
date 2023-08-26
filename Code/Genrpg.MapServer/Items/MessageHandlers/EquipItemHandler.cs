using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class EquipItemHandler : BaseServerMapMessageHandler<EquipItem>
    {

        private IInventoryService _inventoryService = null;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, EquipItem message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                pack.SendError(gs, obj, "Only players can equip items");
                return;
            }

            if (!_inventoryService.EquipItem(gs, ch, message.ItemId, message.EquipSlot))
            {
                pack.SendError(gs, obj, "You can't equip that there");
                return;
            }
        }
    }
}
