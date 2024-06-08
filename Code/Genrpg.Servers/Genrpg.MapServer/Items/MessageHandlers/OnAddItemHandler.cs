using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Inventory.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Items.MessageHandlers
{
    public class OnAddItemHandler : BaseMapObjectServerMapMessageHandler<OnAddItem>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnAddItem message)
        {
            obj.AddMessage(message);
        }
    }
}
