using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;

namespace Genrpg.RequestServer.Rewards.RewardHelpers.Core
{
    /// <summary>
    /// Give out web rewards for things that we want to load incrementally rather than huge items.
    /// </summary>
    /// <typeparam name="TParent"></typeparam>
    /// <typeparam name="TChild"></typeparam>
    public abstract class BaseAsyncOwnerQuantityRewardHelper<TParent, TChild> : BaseAsyncRewardHelper where TParent : OwnerQuantityObjectList<TChild> where TChild : OwnerQuantityChild, IId, new()
    {      
        public override async Task GiveRewardsAsync(WebContext context, long entityId, long quantity, object extraData = null)
        {
            await _serverRewardService.AddQuantity<TChild>(context, entityId, quantity);
        }
    }
}
