﻿using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Stats
{
    public class StatUpdHandler : BaseClientMapMessageHandler<StatUpd>
    {
        protected override void InnerProcess(StatUpd msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                return;
            }

            unit.Stats.UpdateFromSnapshot(msg.Dat);
            _dispatcher.Dispatch(msg);
        }
    }
}
