using Amazon.Runtime.Internal.Util;
using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Users.Settings;
using Genrpg.Shared.UserStats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.PlayerData.LoadUpdateHelpers
{
    public class CoreUserLoadUpdater : IUserLoadUpdater
    {
        private IGameData _gameData;

        public int Order => 1;

        public Type GetKey() { return GetType(); }


        public async Task Update(WebContext context, List<IUnitData> unitData)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>();

            NewUserSettings newUserSettings = _gameData.Get<NewUserSettings>(context.user);

            if (userData.Stats.Get(UserStats.CreditsMult) == 0)
            {
                userData.Coins.Add(UserCoinTypes.Gems, newUserSettings.StartGems);
                userData.Coins.Add(UserCoinTypes.Credits, newUserSettings.StartCredits);    
                userData.Coins.Add(UserCoinTypes.Energy, newUserSettings.StartEnergy);
            }


            RaiseToMinStat(userData, UserStats.CreditsMult, newUserSettings.StartCredits);

        }

        protected void RaiseToMinStat(CoreUserData userData, long userStatId, long minValue)
        {

            if (userData.Stats.Get(userStatId) < minValue)
            {
                userData.Stats.Set(userStatId, minValue);
            }
        }
    }
}
