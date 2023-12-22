using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.UserCoins.PlayerData;

namespace Genrpg.Shared.Users.Loaders
{
    public class UserCoinDataLoader : OwnerDataLoader<UserCoinData, UserCoinStatus, UserCoinApi>
    {
        protected override bool IsUserData() { return true; }
    }
}
