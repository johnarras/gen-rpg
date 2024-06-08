using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinRewardHelper : IRewardHelper
    {
        private IUserCoinService _userCoinService = null;
        public bool GiveReward(IRandom rand, Character ch, long entityId, long quantity, object extraData = null)
        {
            if (quantity < 1)
            {
                return false;
            }
            _userCoinService.Add(ch, entityId, quantity);
            return true;
        }

        public long GetKey() { return EntityTypes.UserCoin; }

    }
}
