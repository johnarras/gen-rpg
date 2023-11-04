
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.ServerShared.CloudMessaging.Constants;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.MapServer.Messages;
using Genrpg.ServerShared.CloudMessaging.Servers.PlayerServer.Messages;

namespace Genrpg.MapServer.Movement.MessageHandlers
{
    public class UpdPosHandler : BaseServerMapMessageHandler<UpdatePos>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, UpdatePos message)
        {
            obj.X = message.GetX();
            obj.Y = message.GetY();
            obj.Z = message.GetZ();
            obj.Rot = message.GetRot();
            obj.Speed = message.GetSpeed();
            obj.PrevZoneId = obj.ZoneId;
            obj.ZoneId = message.ZoneId;

            if (obj is Character ch)
            {
                if (obj.PrevZoneId != obj.ZoneId)
                {
                    _cloudMessageService.SendMessage(CloudServerNames.Player, new PlayerEnterZone() { Id = ch.Id, ZoneId = ch.ZoneId });
                }


                if ((DateTime.UtcNow - ch.LastServerStatTime).TotalSeconds > 5)
                {
                    MapMessageService serverMessageService = _messageService as MapMessageService;

                    ServerMessageCounts counts = serverMessageService.GetCounts();

                    counts.MapCounts = _objectManager.GetCounts();

                    obj.AddMessage(counts);
                    ch.LastServerStatTime = DateTime.UtcNow;
                }
            }
            _objectManager.UpdatePosition(gs, obj, message.GetKeysDown());

        }
    }
}
