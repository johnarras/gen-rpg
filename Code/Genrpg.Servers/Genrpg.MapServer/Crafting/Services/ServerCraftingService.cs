using Genrpg.MapServer.Items.Services;
using Genrpg.MapServer.Trades.Services;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Constants;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crafting.Messages;
using Genrpg.Shared.Crafting.PlayerData.Crafting;
using Genrpg.Shared.Crafting.PlayerData.Recipes;
using Genrpg.Shared.Crafting.Services;
using Genrpg.Shared.Crafting.Settings.Crafters;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Genrpg.MapServer.Crafting.Services
{

    public interface IServerCraftingService : IInjectable
    {
        CraftingResult CraftItem(IRandom rand, CraftingItemData data, Character ch, bool sendUpdates = false);
        UseItemResult LearnRecipe(IRandom rand, Character ch, Item recipeItem);
        Item GenerateRecipeReward(IRandom rand, long level);
    }

    public class ServerCraftingService : IServerCraftingService
    {
        private IGameData _gameData = null;
        private IInventoryService _inventoryService = null;
        private ITradeService _tradeService = null;
        private ISharedCraftingService _sharedCraftingService = null;
        private IItemGenService _itemGenService = null;

        public CraftingResult CraftItem(IRandom rand, CraftingItemData data, Character ch, bool sendUpdates = false)
        {
            return _tradeService.SafeModifyObject(ch, delegate { return CraftItemInternal(rand, data, ch, sendUpdates); },
                new CraftingResult());
        }

        private CraftingResult CraftItemInternal (IRandom rand, CraftingItemData data, Character ch, bool sendUpdates = false)
        { 
            CraftingResult result = new CraftingResult();
            CraftingStats stats = _sharedCraftingService.CalculateStatsFromReagents(rand, ch, data);

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


            ValidityResult validResult = _sharedCraftingService.HasValidReagents(rand, ch, data, ch);

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


            long crafterTypeId = _sharedCraftingService.GetCrafterTypeFromRecipe(rand, ch, data.RecipeTypeId, data.ScalingTypeId);

            CraftingData crafterData = ch.Get<CraftingData>();

            CraftingStatus crafterStatus = crafterData.Get(crafterTypeId);

            if (crafterStatus == null)
            {
                result.Message = "Unknown crafter type";
                return result;
            }

            int crafterLevel = crafterStatus.Get(CraftingConstants.CraftingSkill);

            RecipeData recipeData = ch.Get<RecipeData>();

            RecipeStatus recipeStatus = recipeData.Get(data.RecipeTypeId);

            if (recipeStatus == null)
            {
                result.Message = "Unknown recipe";
                return result;
            }

            int recipeSkillLevel = recipeStatus.Get();

            int maxCraftableLevel = Math.Min(crafterLevel, recipeSkillLevel) + CraftingConstants.ExtraCraftingLevelAllowed;

            int levelDiff = maxCraftableLevel - recipeSkillLevel;

            int recipeSkillGainChance = GetGainPercentChanceFromLevelDiff(recipeSkillLevel - stats.Level);

            if (rand.NextDouble() * 100 < recipeSkillGainChance && recipeStatus.Get() < recipeStatus.GetMaxLevel())
            {
                recipeStatus.AddLevel(1);
            }

            int crafterSkillGainChance = GetGainPercentChanceFromLevelDiff(crafterLevel - stats.Level);

            if (rand.NextDouble() * 100 < crafterSkillGainChance)
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
                _inventoryService.RemoveItemQuantity(ch, reagent.ItemId, reagent.Quantity);
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
                Name = _itemGenService.GenerateName(rand, stats.EntityId, stats.Level, stats.QualityTypeId, allReagents),
            };


            // Now add the stats that were determined above.
            item.Effects = new List<ItemEffect>();
            if (stats.Stats != null)
            {
                foreach (CraftingStat stat in stats.Stats)
                {
                    ItemEffect ieff = new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = stat.Id, Quantity = stat.Val };
                    item.Effects.Add(ieff);
                }
            }

            result.Succeeded = true;
            result.CraftedItem = item;
            _inventoryService.AddItem(ch, item, true);
            return result;

        }

        /// <summary>
        /// Get the chance to get a skill gain, levelDiff is mySkillLevel-tarGet
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
        public Item GenerateRecipeReward(IRandom rand, long level)
        {

            return null;
        }


        public UseItemResult LearnRecipe(IRandom rand, Character ch, Item recipeItem)
        {
            UseItemResult res = new UseItemResult() { ItemUsed = recipeItem, Success = false };

            ItemProc proc = recipeItem.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Recipe);

            if (proc == null)
            {
                res.Message = "This is not a recipe item.";
                return res;
            }


            ItemType itype = _gameData.Get<ItemTypeSettings>(ch).Get(recipeItem.ItemTypeId);
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
            if (_gameData.Get<CraftingSettings>(ch) == null)
            {
                res.Message = "Missing basic crafting info";
                return res;
            }

            if (status.Get() < recipeItem.Level - _gameData.Get<CraftingSettings>(ch).LootLevelIncrement)
            {
                res.Message = "You need to have " + (recipeItem.Level - _gameData.Get<CraftingSettings>(ch).LootLevelIncrement) +
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
