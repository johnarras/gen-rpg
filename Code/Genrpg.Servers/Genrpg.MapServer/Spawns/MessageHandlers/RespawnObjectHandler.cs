using Genrpg.MapServer.MapMessaging;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class RespawnObjectHandler : BaseServerMapMessageHandler<RespawnObject>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, RespawnObject message)
        {
            _objectManager.SpawnObject(gs, message.Spawn);
        }
    }
}
