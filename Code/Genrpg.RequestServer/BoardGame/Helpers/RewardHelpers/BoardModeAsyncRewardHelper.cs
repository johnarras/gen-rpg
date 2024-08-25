using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.RequestServer.BoardGame.Helpers.RewardHelpers
{
    public class BoardModeAsyncRewardHelper : IAsyncRewardHelper
    {
        public long GetKey() { return EntityTypes.BoardMode; }
   
        public async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData = null)
        {
            await Task.CompletedTask;
        }
    }
}
