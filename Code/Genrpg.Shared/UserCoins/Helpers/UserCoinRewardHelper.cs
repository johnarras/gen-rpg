using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.UserCoins.Helpers
{

    public class UserCoinRewardHelper : IRewardHelper
    {
        public long GetKey() { return EntityTypes.UserCoin; }

        public bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null)
        {
            CoreUserData userData = unit.Get<CoreUserData>();

            userData.Coins.Add(entityId, quantity);

            return true;
        }
    }
}
