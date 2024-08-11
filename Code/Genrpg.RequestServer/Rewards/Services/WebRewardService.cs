using Genrpg.RequestServer.Core;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Entities;
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

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }
        public async Task GiveRewardsAsync(WebContext context, List<SpawnResult> spawnResults)
        {
            await Task.CompletedTask;
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
    }
}
