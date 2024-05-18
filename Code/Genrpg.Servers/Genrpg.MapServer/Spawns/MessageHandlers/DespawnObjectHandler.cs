using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class DespawnObjectHandler : BaseServerMapMessageHandler<DespawnObject>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, DespawnObject message)
        {
            obj.AddMessage(message);
        }
    }
}
