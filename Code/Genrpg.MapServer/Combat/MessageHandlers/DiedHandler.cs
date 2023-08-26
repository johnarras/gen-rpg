using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class DiedHandler : BaseServerMapMessageHandler<Died>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, Died message)
        {

            obj.AddMessage(message);
            if (!_objectManager.GetUnit(obj.Id, out Unit unit))
            {
                return;
            }

            unit.RemoveAttacker(message.UnitId);
            if (unit.TargetId == message.UnitId)
            {
                SetTarget setTarget = unit.GetCachedMessage<SetTarget>(true);
                setTarget.TargetId = "";

                _messageService.SendMessageNear(gs, unit, setTarget);
            }

        }
    }
}
