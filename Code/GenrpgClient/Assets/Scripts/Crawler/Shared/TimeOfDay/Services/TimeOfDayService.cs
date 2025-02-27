using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Zones.Settings;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.Buffs.Constants;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;

namespace Genrpg.Shared.Crawler.TimeOfDay.Services
{
    public interface ITimeOfDayService : IInjectable
    {
        Task UpdateTime(PartyData partyData, ECrawlerTimeUpdateTypes updateType);
    }
    public class TimeOfDayService : ITimeOfDayService
    {

        const int SecondsPerMinute = 60;
        const int MinutesPerHour = 60;
        const int HoursPerDay = 24;

        const int SecondsPerDay = SecondsPerMinute * MinutesPerHour * HoursPerDay;

        private IStatService _statService = null;
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private ICrawlerWorldService _worldService = null;
        private IDispatcher _dispatcher = null;

        public async Task UpdateTime(PartyData partyData, ECrawlerTimeUpdateTypes type)
        {
            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);
            TimeOfDaySettings timeSettings = _gameData.Get<TimeOfDaySettings>(_gs.ch);

            double hoursSpent = 0;

            bool fullHeal = false;

            CrawlerMap map = _worldService.GetMap(partyData.MapId);

            if (type == ECrawlerTimeUpdateTypes.Move)
            {

                long zoneTypeId = map.Get(partyData.MapX, partyData.MapZ, CellIndex.Terrain);

                long regionTypeId = map.Get(partyData.MapX, partyData.MapZ, CellIndex.Region);

                if (regionTypeId > 0)
                {
                    zoneTypeId = regionTypeId;
                }

                ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

                double traversalScale = 1;

                if (zoneType != null && zoneType.TraveralTimeScale > 0)
                {
                    traversalScale = zoneType.TraveralTimeScale;
                }

                if (timeSettings.LevitateSpeedup > 1 && partyData.Buffs.Get(PartyBuffs.Levitate) > 0)
                {
                    traversalScale /= (timeSettings.LevitateSpeedup * partyData.Buffs.Get(PartyBuffs.Levitate));
                }

                hoursSpent = timeSettings.BaseMoveMinutes * 1.0 / MinutesPerHour * traversalScale;

            }
            else if (type == ECrawlerTimeUpdateTypes.CombatRound)
            {
                hoursSpent = timeSettings.CombatRoundMinutes / MinutesPerHour;
            }
            else if (type == ECrawlerTimeUpdateTypes.Rest)
            {
                hoursSpent = timeSettings.RestHours;
                fullHeal = true;
            }
            else if (type == ECrawlerTimeUpdateTypes.Eat)
            {
                hoursSpent = timeSettings.EatHours;
            }
            else if (type == ECrawlerTimeUpdateTypes.Drink)
            {
                hoursSpent = timeSettings.DrinkHours;
            }
            else if(type == ECrawlerTimeUpdateTypes.Rumor)
            {
                hoursSpent += timeSettings.RumorHours;  
            }
            else if (type == ECrawlerTimeUpdateTypes.Tavern)
            {
                hoursSpent = timeSettings.DailyResetHour - partyData.HourOfDay;
                if (hoursSpent < 0)
                {
                    hoursSpent += HoursPerDay;
                }
                fullHeal = true;
            }

            partyData.HourOfDay += (float)hoursSpent;

            while (partyData.HourOfDay > HoursPerDay)
            {
                partyData.HourOfDay -= HoursPerDay;
                partyData.DaysPlayed++;
            }

            foreach (PartyMember member in partyData.GetActiveParty())
            {
                if (member.StatusEffects.MatchAnyBits(-1))
                {
                    continue;
                }

                bool didAdjustStat = false;
                foreach (StatRegenHours hours in timeSettings.RegenHours)
                {
                    long maxVal = member.Stats.Max(hours.StatTypeId);
                    long currVal = member.Stats.Curr(hours.StatTypeId);

                    if (currVal >= maxVal)
                    {
                        continue;
                    }

                    StatRegenFraction fraction = member.RegenFractions.FirstOrDefault(x => x.StatTypeId == hours.StatTypeId);
                    if (fraction == null)
                    {
                        fraction = new StatRegenFraction()
                        {
                            StatTypeId = hours.StatTypeId,
                        };
                        member.RegenFractions.Add(fraction);
                    }

                    float regenPercent = (float)(hoursSpent / hours.RegenHours);

                    if (fullHeal)
                    {
                        regenPercent = 1;
                    }

                    fraction.Fraction += regenPercent * maxVal;

                    long currRegen = (long)fraction.Fraction;
                    fraction.Fraction -= currRegen;

                    if (currRegen > 0)
                    {
                        currVal += currRegen;
                        if (currVal > maxVal)
                        {
                            currVal = maxVal;
                        }
                        _statService.Set(member, fraction.StatTypeId, StatCategories.Curr, currVal);
                        didAdjustStat = true;
                        if (currVal >= maxVal)
                        {
                            member.RegenFractions.Remove(fraction);
                        }
                    }
                }

                if (didAdjustStat)
                {
                    _dispatcher.Dispatch(new RefreshUnitStatus() { Unit = member });
                }
            }

            _dispatcher.Dispatch(new CrawlerUIUpdate());
        }
    }
}
