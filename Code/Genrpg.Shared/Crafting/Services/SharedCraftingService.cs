
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Levels.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Stats.Constants;

namespace Genrpg.Shared.Crafting.Services
{
    public interface ISharedCraftingService : IService
    {
        CraftingStats CalculateStatsFromReagents(GameState gs, Character ch, CraftingItemData data);
        ValidityResult HasValidReagents(GameState gs, Character ch, CraftingItemData data, Character crafter);
        long GetCrafterTypeFromRecipe(GameState gs, Character ch, long recipeTypeId, long scalingTypeId);

    }

    public class SharedCraftingService : ISharedCraftingService
    {
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
        public virtual CraftingStats CalculateStatsFromReagents(GameState gs, Character ch, CraftingItemData data)
        {
            CraftingStats stats = new CraftingStats();

            Dictionary<int, long> statTotals = new Dictionary<int, long>();

            RecipeType recipe = gs.data.GetGameData<RecipeSettings>(ch).GetRecipeType(data.RecipeTypeId);

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

            ScalingType scaleType = gs.data.GetGameData<ScalingTypeSettings>(ch).GetScalingType(data.ScalingTypeId);

            if (scaleType == null)
            {
                stats.Message = "Scaling type " + data.ScalingTypeId + " does not exist";
                return stats;
            }

            stats.ScalingTypeId = scaleType.IdKey;

            ItemType resultItemType = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(recipe.EntityId);

            if (resultItemType == null)
            {
                stats.Message = "Recipe " + data.RecipeTypeId + " output item id " + recipe.EntityId + " does not exist";
                return stats;
            }


            EquipSlot equipSlot = gs.data.GetGameData<EquipSlotSettings>(ch).GetEquipSlot(resultItemType.EquipSlotId);
            if (equipSlot == null)
            {
                stats.Message = " Result item " + resultItemType.IdKey + " equip slot " + resultItemType.EquipSlotId + " is not valid";
                return stats;
            }

            stats.EntityTypeId = recipe.EntityTypeId;
            stats.EntityId = recipe.EntityId;

            double levelTotal = 0; // Sum Quantity*Level
            double qualityTotal = 0; // Sum Quantity*Quality
            double levelQualityTotal = 0; // Sum Quantity*Level*Quality

            if (data.RecipeReagents == null || data.RecipeReagents.Count < 1 || data.PrimaryReagent == null)
            {
                stats.Message = "crafting data was missing base reagents or primary reagent";
                return stats;
            }
            List<FullReagent> allReagents = new List<FullReagent>();
            allReagents.AddRange(data.RecipeReagents);
            allReagents.Add(data.PrimaryReagent);
            if (data.ExtraReagents != null)
            {
                allReagents.AddRange(data.ExtraReagents);
            }


            // Get level and quality totals to calc stats.
            foreach (FullReagent reagent in allReagents)
            {
                levelTotal += reagent.Quantity * reagent.Level;
                qualityTotal += reagent.Quantity * reagent.QualityTypeId / 100.0f;
                levelQualityTotal += reagent.Quantity * reagent.Level * reagent.QualityTypeId / 100.0f;
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

            LevelInfo prevLev = gs.data.GetGameData<LevelSettings>(ch).GetLevel((int)Math.Floor(averageLevel));
            LevelInfo nextLev = gs.data.GetGameData<LevelSettings>(ch).GetLevel((int)Math.Ceiling(averageLevel));
            QualityType prevQuality = gs.data.GetGameData<QualityTypeSettings>(ch).GetQualityType((int)Math.Floor(averageQuality));
            QualityType nextQuality = gs.data.GetGameData<QualityTypeSettings>(ch).GetQualityType((int)Math.Ceiling(averageQuality));
            
            if (nextQuality == null)
            {
                nextQuality = prevQuality;
            }

            Dictionary<long, long> statPercents = new Dictionary<long, long>();

            foreach (FullReagent reagent in allReagents)
            {
                // The core recipe reagents don't provide stats, just level + quality
                if (data.RecipeReagents.Contains(reagent))
                {
                    continue;
                }

                ItemType itype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(reagent.ItemTypeId);
                if (itype == null)
                {
                    continue;
                }

                Dictionary<long, long> currPcts = itype.GetCraftingStatPercents(gs, ch, reagent.Level, reagent.QualityTypeId);
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

            int equipSlotScaling = equipSlot.StatPercent;

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

                StatType statType = gs.data.GetGameData<StatSettings>(ch).GetStatType(key);

                if (statType != null)
                {
                    statTypeScaling = statType.GenScalePct;
                }

                // First take the perceat and scale with equip slot, scaling type and the stat itself.
                double statValDouble = 1.0f * startStatPercent * equipSlotScaling * scaleTypeScalingPct * statTypeScaling / (100 * 100 * 100 * 100.0f);

                // Now scale relative to the (fractional) level and quality.

                statValDouble *= levelStatAmount * qualityStatScale;

                // now it's scaled relative to all of these factors, add 0.5f to round and then set this as the return value.
                Stat stat = new Stat()
                {
                    Id = (short)key,
                };
                stat.Val = (int)Math.Ceiling(statValDouble);
                stats.Stats.Add(stat);
            }
            stats.Level = (long)Math.Floor(averageLevel);
            stats.QualityTypeId = nextQuality.IdKey;
            stats.IsValid = true;
            return stats;
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

        public ValidityResult HasValidReagents(GameState gs, Character ch, CraftingItemData data, Character crafter)
        {

            ValidityResult result = new ValidityResult() { IsValid = false };
            result.Data = data;

            RecipeType rtype = gs.data.GetGameData<RecipeSettings>(ch).GetRecipeType(data.RecipeTypeId);

            if (rtype == null)
            {
                result.Message = "Recipe " + data.RecipeTypeId + " not found";
                return result;
            }
            InventoryData inventory = ch.Get<InventoryData>();

            ItemTypeSettings itemSettings = gs.data.GetGameData<ItemTypeSettings>(ch);

            Dictionary<string, long> itemsUsedDict = new Dictionary<string, long>();

            if (rtype.CrafterTypeId > 0)
            {
                if (data.ExtraReagents == null)
                {
                    result.Message = "Crafting data contained no reagents.";
                    return result;
                }
                foreach (Reagent rreagent in rtype.Reagents)
                {
                    FullReagent dreagent = data.ExtraReagents.FirstOrDefault(x => x.ReagentMappedTo == rreagent);
                    if (dreagent == null)
                    {
                        result.Message = "Could not find reagent data for reagent " + rreagent.Quantity + " of #" + rreagent.EntityId;
                        return result;
                    }

                    // JRAJRA For now only have item reagents. Later on maybe allow other things.
                    if (rreagent.EntityTypeId != EntityTypes.Item)
                    {
                        continue;
                    }

                    ItemType itype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(rreagent.EntityId);

                    if (itype == null)
                    {
                        result.Message = "Could not find item type " + rreagent.EntityId;
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
                ScalingType scaling = gs.data.GetGameData<ScalingTypeSettings>(ch).GetScalingType(data.ScalingTypeId);

                if (scaling == null)
                {
                    result.Message = "Crafting data references invalid scaling type: " + data.ScalingTypeId;
                    return result;
                }


                // Check base reagents...these don't give stats.
                if (data.RecipeReagents == null || data.RecipeReagents.Count < 1)
                {
                    result.Message = "No base reagents found in crafting data";
                    return result;
                }

                if (scaling.BaseReagents == null || scaling.BaseReagents.Count < 1)
                {
                    result.Message = "No base reagents found in scaling data";
                    return result;
                }

                if (scaling.BaseReagents.Count != data.RecipeReagents.Count)
                {
                    result.Message = "Base reagent count and craft reagents used count don't match.";
                    return result;
                }

                List<FullReagent> remainingReagents = new List<FullReagent>(data.RecipeReagents);

                // Check each Base reagent vs what's in the character's inventory.
                // These base reagents must be the basic types. 
                // If we ever have different kinds of wood or ore or something,
                // then these core things will still be used, but those other things
                // can be used for the primary reagent. (like bamboo insted of wood for diff stats)
                foreach (ItemPct scalingReagent in scaling.BaseReagents)
                {
                    ItemType itype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(scalingReagent.ItemTypeId);

                    if (itype == null)
                    {
                        result.Message = "Item type not found: " + scalingReagent.ItemTypeId;
                        return result;
                    }
                    FullReagent baseReagent = remainingReagents.FirstOrDefault(x => x.ItemMappedTo == scalingReagent);
                    if (baseReagent == null)
                    {
                        result.Message = "Base reagent mapping to Item " + itype.Name + " not found";
                        return result;
                    }

                    if (string.IsNullOrEmpty(baseReagent.ItemId))
                    {
                        result.Message = "Base reagent has no ItemId";
                        return result;
                    }

                    Item baseItem = inventory.GetItem(baseReagent.ItemId);

                    if (baseItem == null)
                    {
                        result.Message = "Unit does not contain an item with id " + baseReagent.ItemId;
                        return result;
                    }

                    if (baseItem.ItemTypeId != scalingReagent.ItemTypeId)
                    {
                        result.Message = "Base Data reagent has the incorrect ItemTypeId is: " + baseItem.ItemTypeId + " want: " + scalingReagent.ItemTypeId;
                        return result;
                    }

                    int desiredQuantity = scalingReagent.Percent * rtype.ReagentQuantity / 100;

                    if (baseReagent.Quantity != desiredQuantity)
                    {
                        result.Message = "Quantity desired was " + desiredQuantity + " but reagent had " + baseReagent.Quantity;
                    }

                    remainingReagents.Remove(baseReagent);

                    if (!itemsUsedDict.ContainsKey(baseReagent.ItemId))
                    {
                        itemsUsedDict[baseReagent.ItemId] = 0;
                    }

                    itemsUsedDict[baseReagent.ItemId] += baseReagent.Quantity;


                }


                // Check the primary reagent that gives the core stats.
                // Does 
                if (data.PrimaryReagent == null)
                {
                    result.Message = "Data is missing a core reagent";
                    return result;
                }


                ItemType primaryItemType = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(data.PrimaryReagent.ItemTypeId);

                if (primaryItemType == null)
                {
                    result.Message = "Primary item type: " + data.PrimaryReagent.ItemTypeId + " does not exist";
                    return result;
                }



                // Primary reagents are marked with the 1 = 0x01 flagbit
                List<ItemType> primaryReagents = itemSettings.GetPrimaryReagents();

                if (primaryReagents == null || !primaryReagents.Contains(primaryItemType))
                {
                    result.Message = "Item type " + primaryItemType.Name + " #" + primaryItemType.IdKey + " is not a primary item type";
                    return result;
                }

                if (string.IsNullOrEmpty(data.PrimaryReagent.ItemId))
                {
                    result.Message = "Primary reagent has no ItemId";
                    return result;
                }

                Item primaryItem = inventory.GetItem(data.PrimaryReagent.ItemId);

                if (primaryItem == null)
                {
                    result.Message = "Unit does not contain a primary item with id " + data.PrimaryReagent.ItemId;
                    return result;
                }

                if (primaryItem.ItemTypeId != data.PrimaryReagent.ItemTypeId)
                {
                    result.Message = "Data priamry reagent has the incorrect ItemTypeId is: " + primaryItem.ItemTypeId + " want: " + data.PrimaryReagent.ItemTypeId;
                    return result;
                }

                if (primaryItem.Quantity != rtype.ReagentQuantity)
                {
                    result.Message = "Primary reagent count is " + primaryItem.Quantity + " but should be " + rtype.ReagentQuantity;
                    return result;
                }

                if (!itemsUsedDict.ContainsKey(data.PrimaryReagent.ItemId))
                {
                    itemsUsedDict[data.PrimaryReagent.ItemId] = 0;
                }

                itemsUsedDict[data.PrimaryReagent.ItemId] += data.PrimaryReagent.Quantity;

                if (data.ExtraReagents == null)
                {
                    data.ExtraReagents = new List<FullReagent>();
                }

                // Check all secondary reagents (if any)
                foreach (FullReagent reagent in data.ExtraReagents)
                {
                    ItemType reagentItype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(reagent.ItemTypeId);
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

                    if (reagent.Quantity != rtype.ReagentQuantity)
                    {
                        result.Message = "Secondary reagent has quantity " + reagent.Quantity + " and needs " + rtype.ReagentQuantity;
                        return result;
                    }

                    if (!itemsUsedDict.ContainsKey(reagent.ItemId))
                    {
                        itemsUsedDict[reagent.ItemId] = 0;
                    }

                    itemsUsedDict[reagent.ItemId] += reagent.Quantity;
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

                FullReagent dreagent = data.ExtraReagents.FirstOrDefault(x => x.ItemId == iid);
                if (dreagent == null)
                {
                    result.Message = "Missing data reagents with Id " + iid;
                    return result;
                }

                ItemType itype = gs.data.GetGameData<ItemTypeSettings>(ch).GetItemType(dreagent.ItemTypeId);
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
        public long GetCrafterTypeFromRecipe(GameState gs, Character ch, long recipeTypeId, long scalingTypeId)
        {
            RecipeType rtype = gs.data.GetGameData<RecipeSettings>(ch).GetRecipeType(recipeTypeId);
            if (rtype == null)
            {
                return 0;
            }

            if (rtype.CrafterTypeId > 0)
            {
                return rtype.CrafterTypeId;
            }

            ScalingType scalingType = gs.data.GetGameData<ScalingTypeSettings>(ch).GetScalingType(scalingTypeId);
            if (scalingType == null || scalingType.CrafterTypeId < 1)
            {
                return 0;
            }

            return scalingType.CrafterTypeId;
        }

    }
}
