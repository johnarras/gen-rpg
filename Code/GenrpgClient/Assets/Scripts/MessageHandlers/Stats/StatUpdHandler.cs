﻿using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Stats
{
    public class StatUpdHandler : BaseClientMapMessageHandler<StatUpd>
    {
        protected override void InnerProcess(UnityGameState gs, StatUpd msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                return;
            }

            foreach (SmallStat stat in msg.Dat)
            {
                unit.Stats.Set(stat.GetStatId(), StatCategory.Base, stat.GetMaxVal());
                unit.Stats.Set(stat.GetStatId(), StatCategory.Curr, stat.GetCurrVal());
            }
            gs.Dispatch(msg);
        }
    }
}