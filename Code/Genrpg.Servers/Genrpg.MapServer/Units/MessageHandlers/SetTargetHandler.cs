using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class SetTargetHandler : BaseServerMapMessageHandler<SetTarget>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SetTarget message)
        {

            string targetId = null;
            if (obj is Unit unit)
            {
                if (!string.IsNullOrEmpty(message.TargetId))
                {
                    if (_objectManager.GetUnit(message.TargetId, out Unit targetObject))
                    {
                        targetId = message.TargetId;
                    }
                }
                unit.TargetId = targetId;

                OnSetTarget onSet = obj.GetCachedMessage<OnSetTarget>(true);
                onSet.CasterId = obj.Id;
                onSet.TargetId = targetId;

                _messageService.SendMessageNear(obj, onSet);
            }
        }
    }
}
