using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.UserCoins.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.UserCoins.Helpers
{
    public class UserCoinHelper : BaseEntityHelper<UserCoinSettings,UserCoinType>
    {

        public override long GetKey() { return EntityTypes.UserCoin; }
    }
}
