using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class InterruptCastHandler : BaseServerMapMessageHandler<InterruptCast>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, InterruptCast message)
        {

            ICastTimeMessage castTimeMessage = obj.ActionMessage as ICastTimeMessage;
            if (castTimeMessage != null && castTimeMessage.CastingTime == 0)
            {
                return;
            }

            ICastMessage actorCastMessage = obj.ActionMessage as ICastMessage;

            if (actorCastMessage != null && _objectManager.GetObject(actorCastMessage.TargetId, out MapObject target))
            {
                ICastMessage targetCastMessage = target.OnActionMessage as ICastMessage;

                if (targetCastMessage != null &&
                    targetCastMessage.CasterId == actorCastMessage.CasterId &&
                    targetCastMessage.TargetId == actorCastMessage.TargetId)
                {
                    targetCastMessage.SetCancelled(true);
                }
            }

            if (obj.ActionMessage != null)
            {
                obj.ActionMessage.SetCancelled(true);
                obj.ActionMessage = null;
            }

            OnStopCast stop = obj.GetCachedMessage<OnStopCast>(true);
            stop.CasterId = obj.Id;

            _messageService.SendMessageNear(obj, stop);
        }
    }
}
