﻿
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System.Threading;
using Genrpg.Shared.Targets.Messages;

namespace Assets.Scripts.MessageHandlers.Targets
{
    public class OnSetTargetHandler : BaseClientMapMessageHandler<OnSetTarget>
    {
        protected override void InnerProcess(OnSetTarget msg, CancellationToken token)
        {
            if (_objectManager.GetMapObject(msg.CasterId,out MapObject obj))
            {
                if (obj is Unit unit)
                {
                    unit.TargetId = msg.TargetId;
                }
            }
        }
    }
}
