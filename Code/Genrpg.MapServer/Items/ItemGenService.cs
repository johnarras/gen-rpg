﻿using Genrpg.MapServer.Spawns;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crafting.Settings.Recipes;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Qualities;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Levels.Settings;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Entities;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Genrpg.MapServer.Items
{
    public interface IItemGenService : IService
    {
        Item Generate(GameState gs, ItemGenData genData);
        Item CreateSimpleItem(GameState gs, ItemGenData gd);
        Item GenerateLevelRangeItem(GameState gs, ItemGenData gd);
        string GenerateName(GameState gs, long itemTypeId, long level, long qualityTypeId, List<FullReagent> reagents);

    }

    public class ItemGenService : IItemGenService
    {
        private INameGenService _nameGenService = null;
        private IStatService _statService = null;

        // ACtually generate an item from either an old item or an itemTypeId.
        public virtual Item Generate(GameState gs, ItemGenData genData)
        {
            Item item = null;

            if (genData.oldItem != null)
            {
                item = new Item();
                item.Id = HashUtils.NewGuid();
                item.ItemTypeId = genData.oldItem.ItemTypeId;
                item.QualityTypeId = genData.oldItem.QualityTypeId;
                item.Quantity = genData.oldItem.Quantity;
                item.Name = genData.oldItem.Name;
                item.Level = genData.oldItem.Level;
                if (genData.oldItem.Effects != null)
                {
                    item.Effects = new List<ItemEffect>(genData.oldItem.Effects);
                }

                return item;
            }
            if (genData.QualityTypeId <= 0)
            {
                genData.QualityTypeId = ChooseItemQuality(gs, genData);
            }

            if (genData.ItemTypeId < 1)
            {
                List<RecipeType> recipes = gs.data.GetGameData<RecipeSettings>(null).GetData().Where(x => x.CrafterTypeId < 1).ToList();
                if (recipes.Count < 1)
                {
                    return null;
                }

                RecipeType recipe = recipes[gs.rand.Next() % recipes.Count];
                genData.ItemTypeId = recipe.EntityId;
            }



            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(genData.ItemTypeId);
            if (itype == null)
            {
                return null;
            }

            if (itype.EquipSlotId > 0)
            {
                return GenerateEquipment(gs, genData);
            }
            else
            {
                return GenerateLevelRangeItem(gs, genData);
            }
        }

        public Item CreateSimpleItem(GameState gs, ItemGenData gd)
        {
            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(gd.ItemTypeId);
            if (itype == null)
            {
                return null;
            }


            Item item = new Item();
            item.Id = HashUtils.NewGuid();
            item.ItemTypeId = itype.IdKey;
            item.QualityTypeId = gd.QualityTypeId;
            item.Quantity = gd.Quantity;
            if (item.Quantity > 1 && item.EquipSlotId > 0)
            {
                item.Quantity = 1;
            }

            if (itype.Effects != null)
            {
                item.Effects = new List<ItemEffect>();
                foreach (ItemEffect eff in itype.Effects)
                {
                    if (eff.EntityTypeId == EntityTypes.Stat || eff.EntityTypeId == EntityTypes.StatPct)
                    {
                        continue;
                    }

                    item.Effects.Add(SerializationUtils.FastMakeCopy(eff));
                }
            }
            return item;
        }


        public Item GenerateLevelRangeItem(GameState gs, ItemGenData gd)
        {
            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(gd.ItemTypeId);
            if (itype == null)
            {
                return null;
            }


            Item item = CreateSimpleItem(gs, gd);
            if (item == null)
            {
                return null;
            }

            item.Level = itype.GetRangedLevel(gs, gd.Level);





            return item;
        }


        public string GenerateName(GameState gs, long itemTypeId, long level, long qualityTypeId, List<FullReagent> reagents)
        {

            string badName = "Armor";

            // If the power of the item is < 0.7f
            // the name is ItemBadPrefix + Name

            // Otherwise:

            // Adj Name (Power < 1.3)
            // Name of Noun < (Power < 1.3)
            // Adj Name of Noun
            // Name of Adj Noun
            // Adj Name of Adj Noun

            // Adj can be picked from ItemAdjectives
            // Noun can be picked from ItemNouns
            // Adj can also be a DoubleWord
            // Noun can also be a "the DoubleWord".

            NameList adjList = gs.data.GetGameData<NameSettings>(null).GetNameList("ItemAdjectives");
            NameList nounList = gs.data.GetGameData<NameSettings>(null).GetNameList("ItemNouns");

            NameList doublePrefixList = gs.data.GetGameData<NameSettings>(null).GetNameList("ItemDoublePrefix");
            NameList doubleSuffixList = gs.data.GetGameData<NameSettings>(null).GetNameList("ItemDoubleSuffix");

            NameList badItemList = gs.data.GetGameData<NameSettings>(null).GetNameList("ItemBadPrefix");

            List<WeightedName> itemNameList = null;

            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(itemTypeId);
            if (itype == null)
            {
                return badName;
            }
            badName = itype.Name;

            EquipSlot equipSlot = gs.data.GetGameData<EquipSlotSettings>(null).GetEquipSlot(itype.EquipSlotId);

            if (equipSlot == null)
            {
                return badName;
            }

            ItemType itemType = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(itemTypeId);

            if (itemType != null && itemType.Names != null)
            {
                itemNameList = itemType.Names;
            }

            if (itemNameList == null || itemNameList.Count < 1)
            {
                return badName;
            }

            string itemName = _nameGenService.PickWord(gs, itemNameList, gs.rand.Next());

            string adj1 = _nameGenService.PickWord(gs, adjList.Names, gs.rand.Next());
            string adj1Prefix = "";
            if (!string.IsNullOrEmpty(adj1))
            {
                adj1Prefix = adj1.Substring(0, 3);
            }

            string adj2 = _nameGenService.PickWord(gs, adjList.Names, gs.rand.Next(), "", adj1Prefix);
            string adj2Prefix = "";
            if (!string.IsNullOrEmpty(adj2))
            {
                adj2Prefix = adj2.Substring(0, 3);
            }

            string suffixName = _nameGenService.PickWord(gs, nounList.Names, gs.rand.Next(), "", adj2Prefix);
            string badPrefix = _nameGenService.PickWord(gs, badItemList.Names, gs.rand.Next());

            if (gs.rand.Next() % 6 == 0 && reagents != null && reagents.Count > 0)
            {
                FullReagent nameReagent = reagents[gs.rand.Next() % reagents.Count];
                if (nameReagent != null)
                {
                    ItemType reagentItemType = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(nameReagent.ItemTypeId);
                    if (reagentItemType != null && !string.IsNullOrEmpty(reagentItemType.GenName))
                    {
                        if (gs.rand.Next() % 3 == 0)
                        {
                            adj1 = reagentItemType.GenName;
                        }
                        else if (gs.rand.Next() % 2 == 0)
                        {
                            adj2 = reagentItemType.GenName;
                        }
                        else
                        {
                            suffixName = reagentItemType.GenName;
                        }
                    }
                }

            }

            string doublePrefix = _nameGenService.PickWord(gs, doublePrefixList.Names, gs.rand.Next());
            string doubleSuffix = _nameGenService.PickWord(gs, doubleSuffixList.Names, gs.rand.Next(), doublePrefix);

            if (!string.IsNullOrEmpty(doubleSuffix))
            {
                doubleSuffix = doubleSuffix.ToLower();
            }
            if (gs.rand.Next() % 7 == 1)
            {
                int val = gs.rand.Next() % 3;
                if (val == 0)
                {
                    adj1 = doublePrefix + doubleSuffix;
                }
                else if (val == 1)
                {
                    adj2 = doublePrefix + doubleSuffix;
                }
                else if (val == 2)
                {
                    suffixName = "the " + doublePrefix + doubleSuffix;
                }
            }



            if (qualityTypeId <= QualityTypes.Common)
            {
                return badPrefix + " " + itemName;
            }

            if (qualityTypeId <= QualityTypes.Uncommon || gs.rand.Next() % 10 == 4)
            {
                if (gs.rand.Next() % 2 == 0)
                {
                    return adj1 + " " + itemName;
                }
                else
                {
                    return itemName + " of " + suffixName;
                }
            }


            // Adj Name of Noun
            if (gs.rand.Next() % 3 != 2)
            {
                return adj1 + " " + itemName + " of " + suffixName;
            }
            // Name of Adj Noun
            else if (gs.rand.Next() % 2 == 1)
            {
                string theSuffix = "";
                if (suffixName != null && suffixName.IndexOf("the ") == 0)
                {
                    suffixName = suffixName.Substring(4);
                    theSuffix = "the ";
                }

                return itemName + " of " + theSuffix + adj2 + " " + suffixName;
            }
            // Adj name of Adj Noun
            else
            {
                string theSuffix = "";
                if (suffixName != null && suffixName.IndexOf("the ") == 0)
                {
                    suffixName = suffixName.Substring(4);
                    theSuffix = "the ";
                }

                return adj1 + " " + itemName + " of " + theSuffix + adj2 + " " + suffixName;

            }

        }

        public Item GenerateEquipment(GameState gs, ItemGenData genData)
        {
            ItemType itype = gs.data.GetGameData<ItemTypeSettings>(null).GetItemType(genData.ItemTypeId);
            if (itype == null)
            {
                return null;
            }
            if (itype.Name == null)
            {
                return null;
            }

            QualityType qualityType = gs.data.GetGameData<QualityTypeSettings>(null).GetQualityType(genData.QualityTypeId);
            if (qualityType == null)
            {
                return null;
            }

            RecipeType rtype = gs.data.GetGameData<RecipeSettings>(null).GetData().FirstOrDefault(x => x.EntityTypeId == EntityTypes.Item && x.EntityId == genData.ItemTypeId);

            if (rtype.CrafterTypeId > 0)
            {
                return null;
            }

            List<ScalingType> scalingTypes = gs.data.GetGameData<ScalingTypeSettings>(null).GetData();
            if (scalingTypes.Count < 1)
            {
                return null;
            }


            EquipSlot eqSlot = gs.data.GetGameData<EquipSlotSettings>(null).GetEquipSlot(itype.EquipSlotId);
            if (eqSlot == null)
            {
                return null;
            }

            // Type of armor weapon
            ScalingType scalingType = scalingTypes[gs.rand.Next() % scalingTypes.Count];

            List<StatType> primaryStats = _statService.GetAttackStatTypes(gs, null);

            if (primaryStats == null || primaryStats.Count < 1)
            {
                return null;
            }

            // Base stat applied.
            StatType coreStat = primaryStats[gs.rand.Next() % primaryStats.Count];


            Dictionary<long, long> statTotals = new Dictionary<long, long>();

            // Items get 3 things.
            // 1. Some stamina.
            AddStatTotal(statTotals, StatTypes.Stamina, scalingType.DefPct);
            // 2. Some core stat
            AddStatTotal(statTotals, coreStat.IdKey, scalingType.AttPct);
            // 3. Maybe another stat like armor/resist/crit...only 1 defined here for simplicity.

            if (scalingType.AddStats != null)
            {
                foreach (StatPct ast in scalingType.AddStats)
                {
                    AddStatTotal(statTotals, ast.StatTypeId, ast.Percent);
                }
            }
            if (scalingType.OtherPct > 0)
            {
                long numExtraEffects = genData.QualityTypeId / 2;


                int sameStatPct = gs.data.GetGameData<ItemTypeSettings>(null).GenSameStatPercent;
                int sameStatBonus = gs.data.GetGameData<ItemTypeSettings>(null).GenSameStatBonusPct;

                int numBaseScale = 0;
                int numNewEffects = 0;

                for (int i = 0; i < numExtraEffects; i++)
                {
                    if (gs.rand.Next() % 100 < sameStatPct)
                    {
                        numBaseScale++;
                    }
                    else
                    {
                        numNewEffects++;
                    }
                }

                int totalScale = numBaseScale * sameStatBonus * scalingType.OtherPct / 100;

                List<long> keys = statTotals.Keys.ToList();
                if (totalScale > 0)
                {
                    foreach (long key in keys)
                    {
                        long currPct = statTotals[key];
                        long newPct = currPct * (100 + totalScale) / 100;
                        statTotals[key] = newPct;
                    }
                }

                List<StatType> fixedStats = _statService.GetFixedStatTypes(gs, null);

                List<StatType> otherStats = fixedStats.Where(x => !keys.Contains(x.IdKey)).ToList();

                for (int i = 0; i < numNewEffects && otherStats.Count > 0; i++)
                {
                    StatType otherStat = otherStats[gs.rand.Next() % otherStats.Count];
                    AddStatTotal(statTotals, otherStat.IdKey, scalingType.OtherPct);
                }

            }

            long baseStat = 10;
            LevelInfo levelData = gs.data.GetGameData<LevelSettings>(null).GetLevel(genData.Level);
            if (levelData != null)
            {
                baseStat = levelData.StatAmount;
            }

            int qualityPct = qualityType.ItemStatPct;
            int eqSlotPct = eqSlot.StatPercent;

            int globalPct = gs.data.GetGameData<ItemTypeSettings>(null).GenGlobalScalingPercent;
            if (globalPct < 5)
            {
                globalPct = 5;
            }


            List<ItemEffect> effs = new List<ItemEffect>();

            foreach (long key in statTotals.Keys)
            {
                long val = statTotals[key];
                if (val == 0)
                {
                    continue;
                }

                StatType stype = gs.data.GetGameData<StatSettings>(null).GetStatType(key);

                if (stype == null)
                {
                    continue;
                }

                // Scaling factor ends up being baseStat*equipSlotPct*qualityPct*globalPct

                long finalVal = baseStat * qualityPct * globalPct * eqSlot.StatPercent * val * stype.GenScalePct / (100L * 100 * 100 * 100 * 100L);
                if (finalVal == 0)
                {
                    finalVal = 1;
                }

                if (finalVal != 0)
                {
                    ItemEffect eff = new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = key, Quantity = finalVal };
                    effs.Add(eff);
                }
            }



            Item item = null;
            item = new Item();
            item.Id = HashUtils.NewGuid();
            item.ItemTypeId = itype.IdKey;
            item.QualityTypeId = genData.QualityTypeId;
            item.Level = genData.Level;
            item.Quantity = 1;
            item.ScalingTypeId = scalingType.IdKey;
            item.Effects = effs;
            item.Name = GenerateName(gs, itype.IdKey, item.Level, item.QualityTypeId, new List<FullReagent>());

            return item;

        }

        private void AddStatTotal(Dictionary<long, long> statTotals, long statTypeId, long amount)
        {
            if (statTypeId < 1 || amount == 0)
            {
                return;
            }

            if (!statTotals.ContainsKey(statTypeId))
            {
                statTotals[statTypeId] = 0;
            }

            statTotals[statTypeId] += amount;
        }

        public long ChooseItemQuality(GameState gs, ItemGenData genData)
        {
            if (gs.data.GetGameData<QualityTypeSettings>(null).GetData() == null)
            {
                return QualityTypes.Common;
            }

            double chanceTotal = 0.0f;

            // Look at all qualitites of the appropriate level that are >= quality to genData.QualityTypeId.
            foreach (QualityType qt in gs.data.GetGameData<QualityTypeSettings>(null).GetData())
            {
                if (qt.ItemMinLevel <= genData.Level && qt.IdKey >= genData.QualityTypeId)
                {
                    chanceTotal += qt.ItemSpawnWeight;
                }
            }

            // No options return Common or the input quality.
            if (chanceTotal >= 0.0001f)
            {
                double chanceChosen = gs.rand.NextDouble() * chanceTotal;

                foreach (QualityType qt in gs.data.GetGameData<QualityTypeSettings>(null).GetData())
                {
                    if (qt.ItemMinLevel <= genData.Level && qt.IdKey >= genData.QualityTypeId)
                    {
                        chanceChosen -= qt.ItemSpawnWeight;
                    }
                    if (chanceChosen <= 0)
                    {
                        return qt.IdKey;
                    }
                }
            }

            if (genData.QualityTypeId > 0)
            {
                return genData.QualityTypeId;
            }

            return QualityTypes.Common;
        }


        protected void AddItemReagentFromReagent(GameState gs, Reagent reagent, List<FullReagent> reagents, ItemGenData gd)
        {
            if (reagent.EntityTypeId == EntityTypes.Item)
            {
                FullReagent ir = new FullReagent();
                ir.ItemTypeId = reagent.EntityId;
                ir.Quantity = reagent.Quantity;
                ir.QualityTypeId = gd.QualityTypeId;
                ir.Level = gd.Level;
                reagents.Add(ir);
                return;
            }
        }


    }
}
