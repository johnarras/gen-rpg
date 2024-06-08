using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class RespawnObjectHandler : BaseMapObjectServerMapMessageHandler<RespawnObject>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, RespawnObject message)
        {
            _objectManager.SpawnObject(rand, message.Spawn);
        }
    }
}
