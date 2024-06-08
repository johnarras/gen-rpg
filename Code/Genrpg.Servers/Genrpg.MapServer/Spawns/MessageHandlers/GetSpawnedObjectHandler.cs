using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class GetSpawnedObjectHandler : BaseMapObjectServerMapMessageHandler<GetSpawnedObject>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, GetSpawnedObject message)
        {
            if (!_objectManager.GetObject(message.ObjId, out MapObject mapObj))
            {
                return;
            }

            _messageService.SendMessage(mapObj, new SendSpawn() { ToObjId = obj.Id });
        }
    }
}
