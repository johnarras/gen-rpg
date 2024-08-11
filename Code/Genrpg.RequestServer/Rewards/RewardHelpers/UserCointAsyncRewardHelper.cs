using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Users.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Rewards.RewardHelpers
{
    public class UserCointAsyncRewardHelper : IAsyncRewardHelper
    {
        public long GetKey() { return EntityTypes.UserCoin; }

        public async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData = null)
        {
            CoreUserData userData = await context.GetAsync<CoreUserData>(); 

            userData.Coins.Add(entityId, quantity);
        }
    }
}
