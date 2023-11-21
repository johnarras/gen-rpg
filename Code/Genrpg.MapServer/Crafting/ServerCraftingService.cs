using Genrpg.MapServer.Items;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crafting.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Crafting
{

    public interface IServerCraftingService : IService
    {
        CraftingResult CraftItem(GameState gs, CraftingItemData data, Character ch, bool sendUpdates = false);
        UseItemResult LearnRecipe(GameState gs, Character ch, Item recipeItem);
        Item GenerateRecipeReward(GameState gs, long level);
    }

    public class ServerCraftingService : IServerCraftingService
    {
        private IInventoryService _inventoryService = null;
        private ISharedCraftingService _sharedCraftingService = null    ;
        private IItemGenService _itemGenService = null;
        public CraftingResult CraftItem(GameState gs, CraftingItemData data, Character ch, bool sendUpdates = false)
        {

            CraftingResult result = new CraftingResult();
            CraftingStats stats = _sharedCraftingService.CalculateStatsFromReagents(gs, ch, data);

            if (stats == null)
            {
                result.Message = "Failed to calculate stats";
                return result;
            }

            if (!stats.IsValid)
            {
                result.Message = "Stat calcs were not valid: " + stats.Message;
                return result;
            }


            ValidityResult validResult = _sharedCraftingService.HasValidReagents(gs, ch, data, ch);

            if (validResult == null)
            {
                result.Message = "Failed to create validity result from checking reagents";
                return result;
            }

            if (!validResult.IsValid)
            {
                result.Message = "Reagent validation failed: " + validResult.Message;
                return result;
            }


            long crafterTypeId = _sharedCraftingService.GetCrafterTypeFromRecipe(gs, ch, data.RecipeTypeId, data.ScalingTypeId);

            CrafterData crafterData = ch.Get<CrafterData>();

            CrafterStatus crafterStatus = crafterData.Get(crafterTypeId);

            if (crafterStatus == null)
            {
                result.Message = "Unknown crafter type";
                return result;
            }

            int crafterLevel = crafterStatus.GetLevel(gs, CraftingConstants.CraftingSkill);

            RecipeData recipeData = ch.Get<RecipeData>();

            RecipeStatus recipeStatus = recipeData.Get(data.RecipeTypeId);

            if (recipeStatus == null)
            {
                result.Message = "Unknown recipe";
                return result;
            }

            int recipeSkillLevel = recipeStatus.GetLevel();

            int maxCraftableLevel = Math.Min(crafterLevel, recipeSkillLevel) + CraftingConstants.ExtraCraftingLevelAllowed;

            int levelDiff = maxCraftableLevel - recipeSkillLevel;

            int recipeSkillGainChance = GetGainPercentChanceFromLevelDiff(recipeSkillLevel - stats.Level);

            if (gs.rand.NextDouble() * 100 < recipeSkillGainChance && recipeStatus.GetLevel() < recipeStatus.GetMaxLevel())
            {
                recipeStatus.AddLevel(1);
            }

            int crafterSkillGainChance = GetGainPercentChanceFromLevelDiff(crafterLevel - stats.Level);

            if (gs.rand.NextDouble() * 100 < crafterSkillGainChance)
            {
                crafterStatus.AddSkillPoints(CraftingConstants.CraftingSkill, 1);
            }


            if (stats.Level > maxCraftableLevel)
            {
                result.Message = "Item level is too high for your skill";
                return result;
            }

            // Now remove all reagents. Quantities were checked in the validity check above.
            List<FullReagent> allReagents = data.GetAllReagents();
            foreach (FullReagent reagent in allReagents)
            {
                _inventoryService.RemoveItemQuantity(gs, ch, reagent.ItemId, reagent.Quantity);
            }

            // Create the new item using the level and quality determined above. Name is generated.


            Item item = new Item()
            {
                Id = HashUtils.NewGuid(),
                Level = stats.Level,
                QualityTypeId = stats.QualityTypeId,
                ItemTypeId = stats.EntityId,
                ScalingTypeId = stats.ScalingTypeId,
                Quantity = 1,
                Name = _itemGenService.GenerateName(gs, stats.EntityId, stats.Level, stats.QualityTypeId, allReagents),
            };


            // Now add the stats that were determined above.
            item.Effects = new List<ItemEffect>();
            if (stats.Stats != null)
            {
                foreach (Stat stat in stats.Stats)
                {
                    ItemEffect ieff = new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = stat.Id, Quantity = stat.Val };
                    item.Effects.Add(ieff);
                }
            }

            result.Succeeded = true;
            result.CraftedItem = item;
            return result;

        }

        /// <summary>
        /// Get the chance to get a skill gain, levelDiff is mySkillLevel-targetLevel
        /// </summary>
        /// <param name="levelDiff"></param>
        /// <returns></returns>
        protected int GetGainPercentChanceFromLevelDiff(long levelDiff)
        {
            int gainPercent = 0;
            if (levelDiff >= 20)
            {
                gainPercent = 0;
            }
            else if (levelDiff >= 10)
            {
                gainPercent = 25;
            }
            else if (levelDiff >= 5)
            {
                gainPercent = 75;
            }
            else
            {
                gainPercent = 100;
            }

            return gainPercent;
        }

        /// <summary>
        /// Need a better way to do these recipes. Need scaling and recipe rewards.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="ps"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Item GenerateRecipeReward(GameState gs, long level)
        {

            return null;
        }


        public UseItemResult LearnRecipe(GameState gs, Character ch, Item recipeItem)
        {
            UseItemResult res = new UseItemResult() { ItemUsed = recipeItem, Success = false };

            if (recipeItem.UseEntityTypeId != EntityTypes.Recipe)
            {
                res.Message = "This is not a recipe item.";
                return res;
            }


            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(recipeItem.ItemTypeId);
            if (itype == null)
            {
                res.Message = "Incorrect recipe item";
                return res;
            }

            RecipeData recipeData = ch.Get<RecipeData>();

            RecipeStatus status = recipeData.Get(recipeItem.ItemTypeId);

            if (status == null)
            {
                res.Message = "You don't know this recipe";
                return res;
            }
            if (gs.data.GetGameData<CraftingSettings>(ch) == null)
            {
                res.Message = "Missing basic crafting info";
                return res;
            }

            if (status.GetLevel() < recipeItem.Level - gs.data.GetGameData<CraftingSettings>(ch).LootLevelIncrement)
            {
                res.Message = "You need to have " + (recipeItem.Level - gs.data.GetGameData<CraftingSettings>(ch).LootLevelIncrement) +
                    " points to learn this recipe.";
                return res;
            }

            status.SetMaxLevel((int)recipeItem.Level);

            res.Success = true;
            res.Message = "Success!";
            return res;
        }
    }
}
