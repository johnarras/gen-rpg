using Genrpg.RequestServer.Core;
using Genrpg.RequestServer.Rewards.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.RequestServer.Rewards.Services
{
    public class WebRewardService : IWebRewardService
    {

        private SetupDictionaryContainer<long,IAsyncRewardHelper> _rewardHelpers = new SetupDictionaryContainer<long, IAsyncRewardHelper> ();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IAsyncRewardHelper GetRewardHelper(long rewardTypeId)
        {
            if (_rewardHelpers.TryGetValue(rewardTypeId, out IAsyncRewardHelper rewardHelper))
            {
                return rewardHelper;
            }
            return null;
        }

        public async Task GiveRewardsAsync(WebContext context, List<Reward> rewards)
        {
            foreach (Reward reward in rewards)
            {
                IAsyncRewardHelper helper = GetRewardHelper(reward.EntityTypeId);
                if (helper != null)
                {
                    await helper.GiveRewardsAsync(context, reward.EntityId, reward.Quantity, reward.EntityId);
                }
            }
        }

        public async Task AddQuantity<TChild>(WebContext context, long entityId, long quantity) where TChild : class, IOwnerQuantityChild, new()
        {
            TChild child = await context.GetAsync<TChild>(entityId);

            child.Quantity += quantity;
            if (child.Quantity < 0)
            {
                child.Quantity = 0;
            }
        }

        public async Task GiveRewardsAsync(WebContext context, List<RewardList> rewardLists)
        {
            foreach (RewardList rewardList in rewardLists)
            {
                await GiveRewardsAsync(context, rewardList.Rewards);
            }
        }
    }
}
