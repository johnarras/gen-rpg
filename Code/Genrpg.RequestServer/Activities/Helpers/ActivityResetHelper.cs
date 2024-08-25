using Genrpg.RequestServer.Activities.Services;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Interfaces;
using Genrpg.Shared.Activities.Settings;
using Genrpg.Shared.GameSettings;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Activities.Helpers
{
    public class ActivityResetHelper : IDailyResetHelper
    {
        protected IGameData _gameData;
        protected IServerActivityService _activityService;

        public int Order => 1000;
        public Type GetKey() { return GetType(); }

        public async Task DailyReset(WebContext context, DateTime currentResetDay, double resetHour)
        {
            await _activityService.DailyReset(context, currentResetDay, resetHour);
            await Task.CompletedTask;
        }
    }
}
