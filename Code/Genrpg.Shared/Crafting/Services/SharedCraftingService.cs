
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crafting.Settings.Recipes;
using System.Threading;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Crafting.Constants;

namespace Genrpg.Shared.Crafting.Services
{
    public interface ISharedCraftingService : IInitializable
    {
        CraftingStats CalculateStatsFromReagents(IRandom rand, Character ch, CraftingItemData data);
        ValidityResult HasValidReagents(IRandom rand, Character ch, CraftingItemData data, Character crafter);
        long GetCrafterTypeFromRecipe(IRandom rand, Character ch, long recipeTypeId, long scalingTypeId);
        int GetReagentQuantity(long recipeTypeId);
    }

    public class SharedCraftingService : ISharedCraftingService
    {

        private IGameData _gameData = null;
        public async Task Initialize(IGameState gs, CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get stats generated when the given crafting stats are used
        /// Add up the stat values given the levels of the items passed in 
        /// and the powers of those items. Then, use the overall power to
        /// scale up the power of this item.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="reagents">List of itemType+Quantity pairs</param>
        /// <param name="equipSlotId">What slot this item will go into...needed since different slots have different stat scaling factors</param>
        /// <returns>List of stat values generated</returns>
        public virtual CraftingStats CalculateStatsFromReagents(IRandom rand, Character ch, CraftingItemData data)
        {
            CraftingStats stats = new CraftingStats();

            Dictionary<int, long> statTotals = new Dictionary<int, long>();

            RecipeType recipe = _gameData.Get<RecipeSettings>(ch).Get(data.RecipeTypeId);

            if (recipe == null)
            {
                stats.Message = "Recipe " + data.RecipeTypeId + " does not exist";
                return stats;
            }

            if (recipe.EntityTypeId != EntityTypes.Item)
            {
                stats.Message = "Recipe " + recipe.IdKey + " does not output an item";
                return stats;
            }

            stats.RecipeTypeId = recipe.IdKey;

            ScalingType scaleType = _gameData.Get<ScalingTypeSettings>(ch).Get(data.ScalingTypeId);

            if (scaleType == null)
            {
                stats.Message = "Scaling type " + data.ScalingTypeId + " does not exist";
                return stats;
            }

            stats.ScalingTypeId = scaleType.IdKey;

            ItemType resultItemType = _gameData.Get<ItemTypeSettings>(ch).Get(recipe.EntityId);

            if (resultItemType == null)
            {
                stats.Message = "Recipe " + data.RecipeTypeId + " output item id " + recipe.EntityId + " does not exist";
                return stats;
            }


            EquipSlot equipSlot = _gameData.Get<EquipSlotSettings>(ch).Get(resultItemType.EquipSlotId);
            if (equipSlot == null)
            {
                stats.Message = " Result item " + resultItemType.IdKey + " equip slot " + resultItemType.EquipSlotId + " is not valid";
                return stats;
            }

            stats.EntityTypeId = recipe.EntityTypeId;
            stats.EntityId = recipe.EntityId;

            List<FullReagent> allStatReagents = new List<FullReagent>();
            allStatReagents.Add(data.BaseScalingReagent);
            allStatReagents.AddRange(data.StatReagents);

            List<FullReagent> allScalingReagents = new List<FullReagent>();
            allScalingReagents.AddRange(allStatReagents);
            allScalingReagents.AddRange(data.LevelQualityReagents);

            double levelTotal = 0; // Sum Quantity*Level
            double qualityTotal = 0; // Sum Quantity*Quality
            double levelQualityTotal = 0; // Sum Quantity*Level*Quality

            // Get level and quality totals to calc stats.
            foreach (FullReagent scalingReagent in allScalingReagents)
            {
                levelTotal += scalingReagent.Quantity * scalingReagent.Level;
                qualityTotal += scalingReagent.Quantity * scalingReagent.QualityTypeId / 100.0f;
                levelQualityTotal += scalingReagent.Quantity * scalingReagent.Level * scalingReagent.QualityTypeId / 100.0f;
            }

            // Eg
            //  10 items at level 50, quality 2.
            // 10 items at level 30, quality 1.

            // LevTotal = 10*50+10*30 = 800
            // QualityTotal = 10*2+10*1 = 30
            // LevelQualityTotal = 10*50*2+10*30*1 = 1000+300 = 1300

            // To get quality, take LevelQualityTotal/LevelTotal = 1300/800 = 1.625
            // To get level, take LevelQualityTotal/QualityTotal = 1300/30 = 43.33

            if (levelTotal < 1 || qualityTotal < 1 || levelQualityTotal < 1)
            {
                stats.Message = "Quality/Level sums for the crafted items were not greater than 0";
                return stats;
            }

            // LQT/QT
            double averageLevel = levelQualityTotal / qualityTotal;

            // LQT/LT
            double averageQuality = 1.0f * qualityTotal / levelTotal;

            double levelRemainder = averageLevel - (float)Math.Floor(averageLevel);
            double qualityRemainder = averageQuality - (float)Math.Floor(averageQuality);

            LevelInfo prevLev = _gameData.Get<LevelSettings>(ch).Get((int)Math.Floor(averageLevel));
            LevelInfo nextLev = _gameData.Get<LevelSettings>(ch).Get((int)Math.Ceiling(averageLevel));
            QualityType prevQuality = _gameData.Get<QualityTypeSettings>(ch).Get((int)Math.Floor(averageQuality));
            QualityType nextQuality = _gameData.Get<QualityTypeSettings>(ch).Get((int)Math.Ceiling(averageQuality));
            
            if (nextQuality == null)
            {
                nextQuality = prevQuality;
            }

            Dictionary<long, long> statPercents = new Dictionary<long, long>();

            foreach (FullReagent reagent in allStatReagents)
            {
                ItemType itype = _gameData.Get<ItemTypeSettings>(ch).Get(reagent.ItemTypeId);
                if (itype == null)
                {
                    continue;
                }

                Dictionary<long, long> currPcts = itype.GetCraftingStatPercents(_gameData, ch, reagent.Level, reagent.QualityTypeId);
                foreach (long key in currPcts.Keys)
                {
                    if (!statPercents.ContainsKey(key))
                    {
                        statPercents[key] = 0;
                    }

                    statPercents[key] += currPcts[key];
                }
            }

            double levelStatAmount = RpgConstants.BaseStat;
            double qualityStatScale = 100.0f;

            if (prevLev == null && nextLev != null)
            {
                prevLev = nextLev;
            }

            if (nextLev == null && prevLev != null)
            {
                nextLev = prevLev;
            }

            if (prevLev != null && nextLev != null)
            {
                levelStatAmount = prevLev.StatAmount * levelRemainder + nextLev.StatAmount * (1 - levelRemainder);
            }

            if (prevQuality == null && nextQuality != null)
            {
                prevQuality = nextQuality;
            }

            if (nextQuality == null && prevQuality != null)
            {
                nextQuality = prevQuality;
            }

            if (prevQuality != null && nextQuality != null)
            {
                qualityStatScale = prevQuality.ItemStatPct * qualityRemainder + nextQuality.ItemStatPct * (1 - qualityRemainder);
            }

            /// Scale quality stat scale down to 1.0f scale.
            qualityStatScale /= 100.0f;

            int equipSlotScaling = recipe.ScalingPct;

            foreach (long key in statPercents.Keys)
            {
                long startStatPercent = statPercents[key];
                int scaleTypeScalingPct = 100;
                if (key == StatTypes.Stamina)
                {
                    scaleTypeScalingPct = scaleType.DefPct;
                }
                else if (key >= StatConstants.PrimaryStatStart && key <= StatConstants.PrimaryStatEnd)
                {
                    scaleTypeScalingPct = scaleType.AttPct;
                }
                else
                {
                    scaleTypeScalingPct = scaleType.OtherPct;
                }

                int statTypeScaling = 100;

                StatType statType = _gameData.Get<StatSettings>(ch).Get(key);

                if (statType != null)
                {
                    statTypeScaling = statType.GenScalePct;
                }

                // First take the perceat and scale with equip slot, scaling type and the stat itself.
                double statValDouble = 1.0f * startStatPercent * equipSlotScaling * scaleTypeScalingPct * statTypeScaling / (100 * 100 * 100 * 100.0f);

                // Now scale relative to the (fractional) level and quality.

                statValDouble *= levelStatAmount * qualityStatScale;

                // now it's scaled relative to all of these factors, add 0.5f to round and then set this as the return value.
                CraftingStat stat = new CraftingStat()
                {
                    Id = (short)statType.IdKey,
                };
                stat.Val = (int)Math.Ceiling(statValDouble);
                stats.Stats.Add(stat);
            }
            stats.Level = (long)Math.Floor(averageLevel);
            stats.QualityTypeId = nextQuality.IdKey;
            stats.ReagentQuantity = GetReagentQuantity(recipe.IdKey);
            stats.IsValid = true;
            return stats;
        }

        public int GetReagentQuantity(long recipeTypeId)
        {
            RecipeSettings settings = _gameData.Get<RecipeSettings>(null);

            RecipeType recipeType = settings.Get(recipeTypeId);

            if (recipeType != null)
            {
                return (int)(Math.Max(CraftingConstants.MinReagentQuantity, 
                    Math.Ceiling(recipeType.ScalingPct * settings.ReagentQuantityPerPercent)));
            }
            return CraftingConstants.BadReagentQuantity;
        }

        private void AddStatPercent(Dictionary<int, long> statDict, int statTypeId, long pct)
        {
            if (statDict == null)
            {
                return;
            }

            if (!statDict.ContainsKey(statTypeId))
            {
                statDict[statTypeId] = 0;
            }

            statDict[statTypeId] += pct;
        }

        public ValidityResult HasValidReagents(IRandom rand, Character ch, CraftingItemData data, Character crafter)
        {

            ValidityResult result = new ValidityResult() { IsValid = false };
            result.Data = data;

            RecipeType rtype = _gameData.Get<RecipeSettings>(ch).Get(data.RecipeTypeId);

            if (rtype == null)
            {
                result.Message = "Recipe " + data.RecipeTypeId + " not found";
                return result;
            }

            int reagentQuantity = GetReagentQuantity(rtype.IdKey);

            InventoryData inventory = ch.Get<InventoryData>();

            ItemTypeSettings itemTypeSettings = _gameData.Get<ItemTypeSettings>(ch);

            Dictionary<string, long> itemsUsedDict = new Dictionary<string, long>();

            if (rtype.ExplicitReagents != null && rtype.ExplicitReagents.Count > 0)
            {
                if (data.StatReagents == null)
                {
                    result.Message = "Crafting data contained no reagents.";
                    return result;
                }
                foreach (Reagent reagent in rtype.ExplicitReagents)
                {
                    FullReagent dreagent = data.StatReagents.FirstOrDefault(x => x.ReagentMappedTo == reagent);
                    if (dreagent == null)
                    {
                        result.Message = "Could not find reagent data for reagent " + reagent.Quantity + " of #" + reagent.EntityId;
                        return result;
                    }

                    // JRAJRA For now only have item reagents. Later on maybe allow other things.
                    if (reagent.EntityTypeId != EntityTypes.Item)
                    {
                        continue;
                    }

                    ItemType itype = _gameData.Get<ItemTypeSettings>(ch).Get(reagent.EntityId);

                    if (itype == null)
                    {
                        result.Message = "Could not find item type " + reagent.EntityId;
                        return result;
                    }

                    if (string.IsNullOrEmpty(dreagent.ItemId))
                    {
                        result.Message = "Data reagent did not map to an item id for item " + itype.Name;
                        return result;
                    }

                    if (!itemsUsedDict.ContainsKey(dreagent.ItemId))
                    {
                        itemsUsedDict[dreagent.ItemId] = 0;
                    }

                    itemsUsedDict[dreagent.ItemId] += dreagent.Quantity;

                }
            }
            else
            {
                ScalingType scaling = _gameData.Get<ScalingTypeSettings>(ch).Get(data.ScalingTypeId);

                if (scaling == null)
                {
                    result.Message = "Crafting data references invalid scaling type: " + data.ScalingTypeId;
                    return result;
                }

                ItemType baseItemType = itemTypeSettings.Get(scaling.BaseItemTypeId);

                if (baseItemType == null)
                {
                    result.Message = "No base reagents found in scaling data";
                    return result;
                }

                List<FullReagent> remainingReagents = new List<FullReagent>();

                // Player has to have the proper itemtype in their inventory

                FullReagent baseReagent = data.BaseScalingReagent;

                if (baseReagent == null)
                {
                    result.Message = "Base Reagent missing";
                    return result;
                }

                if (baseReagent.ItemTypeId != baseItemType.IdKey)
                {
                    result.Message = "Base Data reagent has the incorrect ItemTypeId is: "
                        + baseReagent.ItemTypeId + " want: " + baseItemType.IdKey;
                    return result;
                }

                if (baseReagent.Quantity != reagentQuantity)
                {
                    result.Message = "Improper quantity of base reagent.";
                    return result;
                }

                if (!itemsUsedDict.ContainsKey(baseReagent.ItemId))
                {
                    itemsUsedDict[baseReagent.ItemId] = 0;
                }

                itemsUsedDict[baseReagent.ItemId] += baseReagent.Quantity;

                List<List<FullReagent>> allLists = new List<List<FullReagent>>();
                allLists.Add(data.StatReagents);
                allLists.Add(data.LevelQualityReagents);

                foreach (List<FullReagent> list in allLists)
                {
                    // Check all stat reagents (if any)
                    foreach (FullReagent reagent in list)
                    {
                        ItemType reagentItype = _gameData.Get<ItemTypeSettings>(ch).Get(reagent.ItemTypeId);
                        if (reagentItype == null)
                        {
                            result.Message = "Reagent item type " + reagent.ItemTypeId + " does not exist";
                            return result;
                        }

                        if (string.IsNullOrEmpty(reagent.ItemId))
                        {
                            result.Message = "Reagent has no item id";
                            return result;
                        }

                        Item reagentItem = inventory.GetItem(reagent.ItemId);

                        if (reagentItem == null)
                        {
                            result.Message = "Unit does not contain reagent with id " + reagent.ItemId;
                            return result;
                        }

                        if (reagentItem.ItemTypeId != reagent.ItemTypeId)
                        {
                            result.Message = "Reagent with id " + reagent.ItemId + " has incorrect itemtypeid is: " + reagentItem.ItemTypeId + " want " + reagent.ItemTypeId;
                            return result;
                        }

                        if (reagent.Quantity != reagentQuantity)
                        {
                            result.Message = "Secondary reagent has quantity " + reagent.Quantity + " and needs " + reagentQuantity;
                            return result;
                        }

                        if (!itemsUsedDict.ContainsKey(reagent.ItemId))
                        {
                            itemsUsedDict[reagent.ItemId] = 0;
                        }

                        itemsUsedDict[reagent.ItemId] += reagent.Quantity;
                    }
                }
            }


            // Now check that we have enough of each item we need to craft this recipe.
            foreach (string iid in itemsUsedDict.Keys)
            {
                long quantity = itemsUsedDict[iid];

                Item myItem = inventory.GetItem(iid);

                if (myItem == null)
                {
                    result.Message = "Unit does not contain item " + iid;
                    return result;
                }

                FullReagent dreagent = data.StatReagents.FirstOrDefault(x => x.ItemId == iid);
                if (dreagent == null)
                {
                    result.Message = "Missing data reagents with Id " + iid;
                    return result;
                }

                ItemType itype = _gameData.Get<ItemTypeSettings>(ch).Get(dreagent.ItemTypeId);
                if (itype == null)
                {
                    result.Message = "Missing item type with id " + dreagent.ItemTypeId;
                    return result;
                }

                if (myItem.Quantity < quantity)
                {
                    result.Message = "Need " + quantity + " of " + itype.Name + " but only have " + myItem.QualityTypeId + " L/Q: " +
                        myItem.Level + "/" + myItem.QualityTypeId;
                    return result;
                }

            }

            result.IsValid = true;
            return result;
        }

        /// <summary>
        /// The crafter type for a recipe is from the recipe if it exists, and if not, the
        /// scaling type chosen is the craftertype used.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="ps"></param>
        /// <param name="recipeTypeId"></param>
        /// <param name="scalingTypeId"></param>
        /// <returns></returns>
        public long GetCrafterTypeFromRecipe(IRandom rand, Character ch, long recipeTypeId, long scalingTypeId)
        {
            RecipeType rtype = _gameData.Get<RecipeSettings>(ch).Get(recipeTypeId);
            if (rtype == null)
            {
                return 0;
            }

            if (rtype.CrafterTypeId > 0)
            {
                return rtype.CrafterTypeId;
            }

            ScalingType scalingType = _gameData.Get<ScalingTypeSettings>(ch).Get(scalingTypeId);
            if (scalingType == null || scalingType.CrafterTypeId < 1)
            {
                return 0;
            }

            return scalingType.CrafterTypeId;
        }

    }
}
