using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class OnAddItemHandler : BaseServerMapMessageHandler<OnAddItem>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnAddItem message)
        {
            obj.AddMessage(message);
        }
    }
}
