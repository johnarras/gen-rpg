using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.UserCoins.Helpers
{

    public class UserCoinRewardHelper : IRewardHelper
    {

        private IRewardService _rewardService = null!;
        public long GetKey() { return EntityTypes.UserCoin; }

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData = null)
        {
            if (quantity == 0)
            {
                return false;
            }

            CoreUserData userData = obj.Get<CoreUserData>();
            userData.Coins.Add(entityId, quantity);

            _rewardService.OnAddQuantity(obj, userData, GetKey(), entityId, quantity);

            return true;
        }
    }
}
