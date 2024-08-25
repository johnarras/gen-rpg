using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Interfaces;
using Genrpg.Shared.Activities.PlayerData;
using Genrpg.Shared.Activities.Settings;
using Genrpg.Shared.GameSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Activities.Services
{
    public class ServerActivityService : IServerActivityService
    {
        protected IGameData _gameData;
        public async Task DailyReset(WebContext context, DateTime currentResetDay, double resetHour)
        {

            DateTime currResetTime = currentResetDay.AddHours(resetHour);

            ActivityData data = await context.GetAsync<ActivityData>();

            ActivitySettings settings = _gameData.Get<ActivitySettings>(context.user);

            List<Activity> goodActivities = new List<Activity>();

            foreach (Activity activity in settings.GetData())
            {
                if (activity.MinLevel > 0 && activity.MinLevel > context.user.Level)
                {
                    continue;
                }
                if (activity.MaxLevel > 0 && activity.MaxLevel < context.user.Level)
                {
                    continue;
                }
                goodActivities.Add(activity);
            }

            // Need to do something with activities that arent constantly going on.

            List<long> goodActivityIds = goodActivities.Select(x=>x.IdKey).ToList();    

            List<ActivityStatus> statuses = data.Activities;

            foreach (ActivityStatus status in statuses)
            {
                if (!goodActivityIds.Contains(status.IdKey))
                {
                    data.Activities.Remove(status);
                }
            }

            foreach (long idkey in goodActivityIds)
            {
                ActivityStatus status = data.Activities.FirstOrDefault(x => x.IdKey == idkey);
                if (status == null)
                {
                    status = new ActivityStatus() { IdKey = idkey };
                    data.Activities.Add(status);
                }
            }

            foreach (ActivityStatus status in data.Activities)
            {
                if (status.EndTime > currentResetDay)
                {
                    continue;
                }

                Activity act = goodActivities.FirstOrDefault(x=>x.IdKey == status.IdKey);

                DateTime nextResetTime = currResetTime.AddDays(1);
                if (act.ResetDays == 1)
                {
                    nextResetTime = currResetTime.AddDays(1);
                }
                else if (act.ResetDays == 7)
                {
                    DayOfWeek day = currentResetDay.DayOfWeek;
                    int extraDays = (int)day;
                    nextResetTime = currentResetDay.AddDays(7 - extraDays).AddHours(resetHour);
                }
                else if (act.ResetDays == 30)
                {
                    nextResetTime = new DateTime(currentResetDay.Year, currentResetDay.Month, 1).AddMonths(1).AddHours(resetHour);
                }
                else if (act.ResetDays >= 300)
                {
                    nextResetTime = new DateTime(currentResetDay.Year, 1, 1).AddYears(1).AddHours(resetHour);
                }

                status.EndTime = nextResetTime;
                status.MaxTier = act.MaxRewardTier;

                await ActivityLevelUp(context, status, 0);
            }
        }

        public async Task ActivityLevelUp(WebContext context, ActivityStatus status, int newTier)
        {
            ActivitySettings settings = _gameData.Get<ActivitySettings>(context.user);

            Activity activity = settings.Get(status.IdKey);

            if (activity == null)
            {
                return;
            }

            status.Tier = newTier;


            await Task.CompletedTask;
        }
    }
}
