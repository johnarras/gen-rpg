using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class SendSpawnHandler : BaseMapObjectServerMapMessageHandler<SendSpawn>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, SendSpawn message)
        {
            if (!_objectManager.GetChar(message.ToObjId, out Character ch))
            {
                return;
            }

            if (obj.IsDeleted())
            {
                return;
            }

            _messageService.SendMessage(ch, new OnSpawn(obj));
        }
    }
}
