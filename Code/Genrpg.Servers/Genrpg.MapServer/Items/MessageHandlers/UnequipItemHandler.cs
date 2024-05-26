using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class UnequipItemHandler : BaseServerMapMessageHandler<UnequipItem>
    {

        private IInventoryService _inventoryService = null;
        public override void Setup(GameState gs)
        {
            base.Setup(gs);
        }

        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, UnequipItem message)
        {
            if (!(obj is Character ch))
            {
                return;
            }

            if (!_inventoryService.UnequipItem(ch, message.ItemId))
            {
                pack.SendError(gs, obj, "That item isn't equipped");
                return;
            }
        }
    }
}
