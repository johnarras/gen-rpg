using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Stats.Services;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Stats
{
    public class ServerStatService : SharedStatService
    {

        private IMapMessageService _messageService;
        private IStatService _statService;

        protected override void SendUpdatedStats(GameState gs, Unit unit, List<long> statIdsToSend)
        {
            if (unit.HasFlag(UnitFlags.SuppressStatUpdates) || statIdsToSend.Count < 1)
            {
                return;
            }
            StatUpd upd = new StatUpd()
            {
                UnitId = unit.Id,
            };

            foreach (long statId in statIdsToSend)
            {

                long curr = unit.Stats.Curr(statId);
                long max = unit.Stats.Max(statId);


                upd.AddStat(statId, curr, max);

                if (curr < max &&
                    (unit.RegenMessage == null || unit.RegenMessage.IsCancelled()))
                {
                    unit.RegenMessage = new Regen();

                    _messageService.SendMessage(unit, unit.RegenMessage, StatConstants.RegenTickSeconds);
                }
            }

            _messageService.SendMessageNear(unit, upd);
        }

        private List<StatType> _mutableStats = null;
        public override void RegenerateTick(GameState gs, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds)
        {
            if (unit == null || unit.HasFlag(UnitFlags.IsDead))
            {
                if (unit.RegenMessage != null)
                {
                    unit.RegenMessage.SetCancelled(true);
                    unit.RegenMessage = null;
                }
                return;
            }

            if (_mutableStats == null)
            {
                _mutableStats = _statService.GetMutableStatTypes(gs);
                if (_mutableStats == null)
                {
                    _mutableStats = new List<StatType>();
                }
            }

            long baseStat = 100000;
            LevelData lev = gs.data.GetGameData<LevelSettings>().GetLevel(unit.Level);
            if (lev != null)
            {
                baseStat = lev.StatAmount;
            }
            float spiritMult = 1;

            if (baseStat > 0)
            {
                long spirit = unit.Stats.Curr(StatType.Spirit);
                spiritMult = spirit > baseStat ? (float)Math.Pow(spirit / baseStat, 0.25f) : 1;
            }

            bool haveRegenRemaining = false;

            foreach (StatType st in _mutableStats)
            {
                if (st.RegenSeconds == 0)
                {
                    continue;
                }

                long maxVal = unit.Stats.Max(st.IdKey);
                if (maxVal < 1)
                {
                    continue;
                }

                float regenPct = regenTickTime / st.RegenSeconds;

                if (st.RegenSeconds > 0)
                {
                    regenPct *= spiritMult;
                }

                float currRegenFloat = maxVal * regenPct;

                long currRegen = (long)currRegenFloat;

                if (gs.rand.NextDouble() < currRegenFloat - currRegen)
                {
                    currRegen += Math.Sign(st.RegenSeconds);
                }

                if (currRegen != 0)
                {
                    long oldCurr = unit.Stats.Curr(st.IdKey);
                    long newCurr = MathUtils.Clamp(0, unit.Stats.Curr(st.IdKey) + currRegen, maxVal);
                    if (oldCurr != newCurr)
                    {
                        Set(gs, unit, st.IdKey, StatCategory.Curr, newCurr);
                    }
                }

                if (!haveRegenRemaining && 
                    st.RegenSeconds > 0 && unit.Stats.Curr(st.IdKey) < unit.Stats.Max(st.IdKey) ||
                    st.RegenSeconds < 0 && unit.Stats.Curr(st.IdKey) > 0)
                {
                    haveRegenRemaining = true;
                }
            }

            if (!haveRegenRemaining)
            {
                if (unit.RegenMessage != null)
                {
                    unit.RegenMessage.SetCancelled(true);
                    unit.RegenMessage = null;
                }
            }
        }
    }
}
