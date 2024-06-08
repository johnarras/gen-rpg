using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class SetTargetHandler : BaseUnitServerMapMessageHandler<SetTarget>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, SetTarget message)
        {

            string targetId = null;
            if (!string.IsNullOrEmpty(message.TargetId))
            {
                if (_objectManager.GetUnit(message.TargetId, out Unit targetObject))
                {
                    targetId = message.TargetId;
                }
            }
            unit.TargetId = targetId;

            OnSetTarget onSet = unit.GetCachedMessage<OnSetTarget>(true);
            onSet.CasterId = unit.Id;
            onSet.TargetId = targetId;

            _messageService.SendMessageNear(unit, onSet);
        }
    }
}
