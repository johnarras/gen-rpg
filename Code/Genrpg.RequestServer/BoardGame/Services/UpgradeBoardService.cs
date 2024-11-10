using Amazon.Runtime.Internal.Util;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.UserAbilities.Services;
using Genrpg.Shared.UserAbilities.Settings;
using Genrpg.Shared.UserEnergy.Settings;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.BoardGame.Services
{
    public class UpgradeBoardService : IUpgradeBoardService
    {
        private IGameData _gameData = null;
        private IUserAbilityService _userAbilityService = null;
        public async Task<long> GetTotalUpgradeCost(WebContext context)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();   

            long level = context.user.Level;

            UpgradeBoardSettings upgradeSettings = _gameData.Get<UpgradeBoardSettings>(context.user);

            double upgradeDays = upgradeSettings.GetUpgradeDays(level);

            UserEnergySettings energySettings = _gameData.Get<UserEnergySettings>(context.user);

            double energyPerHour = energySettings.EnergyPerHour(level);

            double totalDicePerDay = upgradeSettings.FullDayEnergyCollectionHours * energyPerHour;

            double totalDice = totalDicePerDay * upgradeDays; // Total dice over the time

            long creditsMult = userData.Stats.Get(UserStats.CreditsMult); // Players credits mult

            BoardGameSettings boardGameSettings = _gameData.Get<BoardGameSettings>(context.user);

            double playGoldMult = boardGameSettings.PlayCreditsMult; // General gameplay mult

            // Total cost ends up being number of days* dice per day * player credit mult * play credit mult
            long totalUpgradeCost = (long)(upgradeDays * totalDicePerDay * creditsMult * playGoldMult);

            if (userData.Stats.Get(UserStats.TotalUpgradeCost) < 1)
            {
                userData.Stats.Set(UserStats.TotalUpgradeCost, totalUpgradeCost);
            }

            return totalUpgradeCost;
        }

        public async Task<long> GetCurrentUpgradeCost(WebContext context, long upgradeIndex, int upgradeTier)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            long totalUpgradeCost = userData.Stats.Get(UserStats.TotalUpgradeCost);

            if (totalUpgradeCost < 1)
            {
                await GetTotalUpgradeCost(context);
            }

            totalUpgradeCost = userData.Stats.Get(UserStats.TotalUpgradeCost);

            UpgradeBoardSettings upgradeSettings = _gameData.Get<UpgradeBoardSettings>(context.user);


            long totalUpgradeCount = _userAbilityService.GetAbilityTotal(context.user, UserAbilityTypes.UpgradeQuantiy, userData.Abilties.Get(UserAbilityTypes.UpgradeQuantiy));

            long totalUpgradeWeight = upgradeSettings.GetTotalUpgradeWeight(totalUpgradeCount);

            long currentUpgradeWeight = upgradeSettings.GetCurrentUpgradeWeight(totalUpgradeWeight, upgradeIndex, upgradeTier);


            double currUpgradeCost = totalUpgradeWeight * currentUpgradeWeight / totalUpgradeWeight;


            return (long)currUpgradeCost;


        }
    }
}
