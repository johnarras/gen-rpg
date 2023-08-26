using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class GetSpawnedObjectHandler : BaseServerMapMessageHandler<GetSpawnedObject>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, GetSpawnedObject message)
        {
            if (!_objectManager.GetObject(message.ObjId, out MapObject mapObj))
            {
                return;
            }

            _messageService.SendMessage(gs, mapObj, new SendSpawn() { ToObjId = obj.Id });
        }
    }
}
