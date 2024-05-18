using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Looting.MessageHandlers
{
    public class ClearLootHandler : BaseServerMapMessageHandler<ClearLoot>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, ClearLoot message)
        {
            obj.AddMessage(message);
        }
    }
}
