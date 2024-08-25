using Genrpg.RequestServer.BoardGame.Helpers.BoardLoadHelpers;
using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Resets.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Resets.Settings;
using Genrpg.Shared.Users.PlayerData;

namespace Genrpg.RequestServer.Resets.Services
{
    public class DailyResetService : IDailyResetService
    {


        protected IGameData _gameData;
        private OrderedSetupDictionaryContainer<Type, IDailyResetHelper> _resetHelpers = new OrderedSetupDictionaryContainer<Type, IDailyResetHelper>();
        //private List<IResetHelper> _helpers = null;

        public async Task DailyReset(WebContext context)
        {
            DateTime currTime = DateTime.UtcNow;
            ResetSettings settings = _gameData.Get<ResetSettings>(context.user);

            CoreUserData userData = await context.GetAsync<CoreUserData>();

            DateTime nextResetTime = userData.LastDailyReset.Date.AddDays(1).AddHours(settings.ResetHour);

            if (nextResetTime > currTime)
            {
                return;
            }

            DateTime resetDay = currTime.Date;
            if (currTime.Hour < settings.ResetHour)
            {
                resetDay = resetDay.AddDays(-1);
            }

            foreach (IDailyResetHelper helper in _resetHelpers.OrderedItems())
            {
                await helper.DailyReset(context, resetDay, settings.ResetHour);
            }

            await Task.CompletedTask;
        }
    }
}
