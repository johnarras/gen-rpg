﻿using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnAddEffectHandler : BaseClientMapMessageHandler<OnAddEffect>
    {
        protected override void InnerProcess(OnAddEffect msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.TargetId,out Unit unit))
            {
                return;
            }

            _dispatcher.Dispatch(msg);
        }
    }
}
