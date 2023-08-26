using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class OnEquipItemHandler : BaseServerMapMessageHandler<OnEquipItem>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnEquipItem message)
        {
            obj.AddMessage(message);
        }
    }
}
