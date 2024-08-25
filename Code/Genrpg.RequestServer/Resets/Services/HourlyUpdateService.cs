using Genrpg.RequestServer.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.UserEnergy.Messages;
using Genrpg.Shared.UserEnergy.Settings;
using Genrpg.Shared.Users.PlayerData;

namespace Genrpg.RequestServer.Resets.Services
{
    public class HourlyUpdateService : IHourlyUpdateService
    {
        private IGameData _gameData;
        public async Task CheckHourlyUpdate(WebContext context)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            int hoursSinceLastUpdate = (int)(DateTime.UtcNow - userData.LastHourlyReset).TotalHours;

            if (hoursSinceLastUpdate < 1)
            {
                return; 
            }

            UserEnergySettings energySettings = _gameData.Get<UserEnergySettings>(context.user);

            long maxStorage = energySettings.GetMaxStorage(context.user.Level);
           
            long energy = userData.Coins.Get(UserCoinTypes.Energy); 

            if (energy >= maxStorage)
            {
                return;
            }

            long totalRegen = (long)Math.Ceiling(hoursSinceLastUpdate * energySettings.HourlyRegenPercent * maxStorage);

            totalRegen = (int)Math.Min(totalRegen, maxStorage - energy);

            userData.Coins.Add(UserCoinTypes.Energy, totalRegen);

            userData.LastHourlyReset = DateTime.UtcNow;

            context.Results.Add(new UpdateUserEnergyResult() { EnergyAdded = totalRegen, LastHourlyReset = userData.LastHourlyReset });

        }
    }
}
