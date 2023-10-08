using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.UserCoins.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.PlayerData.Loaders
{
    public class UserCoinDataLoader : OwnerDataLoader<UserCoinData, UserCoinStatus, UserCoinApi>
    {
        protected override bool IsUserData() { return true; }
    }
}
