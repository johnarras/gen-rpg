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
        public List<Reward> Roll<SI>(IRandom rand, RollData rollData, SI spawnItem) where SI : ISpawnItem
        {
            List<Reward> retval = new List<Reward>();

            Item newItem = _craftingService.GenerateRecipeReward(rand, rollData.Level);
            if (newItem != null)
            {
                Reward sr = new Reward();
                sr.EntityId = newItem.ItemTypeId;
                sr.EntityTypeId = EntityTypes.Item;
                sr.Quantity = 1;
                sr.QualityTypeId = rollData.QualityTypeId;
                sr.Level = rollData.Level;
                retval.Add(sr);
            }
            return retval;
        }
    }
}
