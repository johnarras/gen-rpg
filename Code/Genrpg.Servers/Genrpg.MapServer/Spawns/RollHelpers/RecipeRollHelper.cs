using Genrpg.MapServer.Crafting.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using Microsoft.Extensions.Azure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class RecipeRollHelper : IRollHelper
    {
        public long GetKey() { return EntityTypes.Recipe; }

        private IServerCraftingService _craftingService = null;
        public List<RewardList> Roll<SI>(IRandom rand, RollData rollData, SI spawnItem) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();
            RewardList rewardList = new RewardList();
            Item newItem = _craftingService.GenerateRecipeReward(rand, rollData.Level);
            if (newItem != null)
            {
                Reward rew = new Reward();
                rew.EntityId = newItem.ItemTypeId;
                rew.EntityTypeId = EntityTypes.Item;
                rew.Quantity = 1;
                rew.QualityTypeId = rollData.QualityTypeId;
                rew.Level = rollData.Level;
                rewardList.Rewards.Add(rew);
            }
            return retval;
        }
    }
}
