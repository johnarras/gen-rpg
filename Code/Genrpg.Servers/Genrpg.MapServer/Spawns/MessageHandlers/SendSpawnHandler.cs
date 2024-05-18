using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spawns.MessageHandlers
{
    public class SendSpawnHandler : BaseServerMapMessageHandler<SendSpawn>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SendSpawn message)
        {
            if (!_objectManager.GetChar(message.ToObjId, out Character ch))
            {
                return;
            }
            _messageService.SendMessage(ch, new OnSpawn(obj));
        }
    }
}
