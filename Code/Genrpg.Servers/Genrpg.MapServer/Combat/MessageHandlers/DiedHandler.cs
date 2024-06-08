using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class DiedHandler : BaseUnitServerMapMessageHandler<Died>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, Died message)
        {
            unit.AddMessage(message);
            unit.RemoveAttacker(message.UnitId);

            if (unit.TargetId == message.UnitId)
            {
                SetTarget setTarget = unit.GetCachedMessage<SetTarget>(true);
                setTarget.TargetId = "";

                _messageService.SendMessageNear(unit, setTarget);
            }

        }
    }
}
