using Genrpg.RequestServer.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.UserCoins.Constants;
using Genrpg.Shared.Users.PlayerData;
using Genrpg.Shared.UserStats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Spawns.Helpers
{
    public class UserCoinWebRollHelper : BaseWebRollHelper
    {
        public override long GetKey() { return EntityTypes.UserCoin; }


        protected override async Task<long> GetQuantityMult<SI>(WebContext context, RollData rollData, SI si)
        {
            if (si.EntityId == UserCoinTypes.Gold)
            {
                CoreUserData userData = await context.GetAsync<CoreUserData>();

                return Math.Max(1L, userData.Stats.Get(UserStats.CreditsMult));
            }

            return 1;
        }
    }
}
