using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Messages;
using Genrpg.Shared.Stats.Services;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Stats.Services
{
    public class ServerStatService : SharedStatService
    {

        private IMapMessageService _messageService = null;
        private IStatService _statService = null;

        protected override void SendUpdatedStats(Unit unit, List<long> statIdsToSend)
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
                FullStat fullStat = unit.Stats.GetFullStat(statId);

                upd.AddFullStat(fullStat);

                if (fullStat != null &&
                    fullStat.GetCurr() < fullStat.GetMax() &&
                    (unit.RegenMessage == null || unit.RegenMessage.IsCancelled()))
                {
                    unit.RegenMessage = new Regen();

                    _messageService.SendMessage(unit, unit.RegenMessage, StatConstants.RegenTickSeconds);
                }
            }

            if (upd.Dat.Count > 0)
            {
                _messageService.SendMessageNear(unit, upd);
            }
        }

        private List<StatType> _mutableStats = null;
        public override void RegenerateTick(IRandom rand, Unit unit, float regenTickTime = StatConstants.RegenTickSeconds)
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
                _mutableStats = _statService.GetMutableStatTypes(unit);
                if (_mutableStats == null)
                {
                    _mutableStats = new List<StatType>();
                }
            }

            long baseStat = 100000;
            LevelInfo lev = _gameData.Get<LevelSettings>(unit).Get(unit.Level);
            if (lev != null)
            {
                baseStat = lev.StatAmount;
            }
            float spiritMult = 1;

            if (baseStat > 0)
            {
                spiritMult = 1;
            }

            if (!unit.IsPlayer())
            {
                if (!unit.HasFlag(UnitFlags.DidStartCombat) ||
                    unit.HasFlag(UnitFlags.Evading))
                {
                    spiritMult *= 25;
                }
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

                if (rand.NextDouble() < currRegenFloat - currRegen)
                {
                    currRegen += Math.Sign(st.RegenSeconds);
                }

                if (currRegen != 0)
                {
                    long oldCurr = unit.Stats.Curr(st.IdKey);
                    long newCurr = MathUtils.Clamp(0, unit.Stats.Curr(st.IdKey) + currRegen, maxVal);
                    if (oldCurr != newCurr)
                    {
                        Set(unit, st.IdKey, StatCategories.Curr, newCurr);
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
