using Genrpg.MapServer.Crafting.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
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
        public List<SpawnResult> Roll(GameState gs, RollData rollData, SpawnItem item)
        {
            List<SpawnResult> retval = new List<SpawnResult>();

            Item newItem = _craftingService.GenerateRecipeReward(gs, rollData.Level);
            if (newItem != null)
            {
                SpawnResult sr = new SpawnResult();
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
