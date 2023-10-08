using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Characters.Entities;

using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.UserCoins.Services;
using Genrpg.Shared.Entities.Settings;

namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinRewardHelper : IRewardHelper
    {
        private IUserCoinService _userCoinService;
        public bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null)
        {
            if (quantity < 1)
            {
                return false;
            }
            _userCoinService.Add(gs, ch, entityId, quantity);
            return true;
        }

        public long GetKey() { return EntityType.UserCoin; }

    }
}
