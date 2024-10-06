using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crawler.Loot.Constants;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.LootRanks;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Names.Settings;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Crawler.Combat.Entities;

namespace Genrpg.Shared.Crawler.Loot.Services
{


    public interface ILootGenService : IInjectable
    {

        Item GenerateItem(ItemGenData lootGenData);
        PartyLoot GiveCombatLoot(PartyData party, CrawlerCombatState combatState, CrawlerMap map);
        PartyLoot GiveLoot(PartyData party, CrawlerMap map, LootGenData genData);
        List<string> GenerateItemNames(IRandom rand, int itemCount);
    }
    public class LootGenData
    {
        public long Exp { get; set; }
        public long Gold { get; set; }
        public int ItemCount { get; set; }
        public long Level { get; set; }
        public List<long> QuestItems { get; set; } = new List<long>();
    }

    public class PartyLoot
    {
        public long Gold { get; set; }
        public long Exp { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public List<long> NewQuestItems { get; set; } = new List<long>();
    }

    public class LootGenService : ILootGenService
    {
        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected IClientRandom _rand;
        protected IItemGenService _itemGenService;
        public Item GenerateItem(ItemGenData lootGenData)
        {

            return GenerateEquipment(lootGenData);
        }

        public Item GenerateEquipment(ItemGenData lootGenData)
        {
            long level = lootGenData.Level;

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            LootRankSettings rankSettings = _gameData.Get<LootRankSettings>(null);

            IReadOnlyList<LootRank> ranks = rankSettings.GetData();

            int expectedOffset = (int)(level / rankSettings.LevelsPerQuality + 1);

            expectedOffset = MathUtils.Clamp(1, expectedOffset, ranks.Count - 2);

            List<LootRank> okRanks = new List<LootRank>();

            while (expectedOffset < ranks.Count - 2 && _rand.NextDouble() < rankSettings.ExtraQualityChance)
            {
                expectedOffset++;
            }

            for (int index = expectedOffset - 1; index <= expectedOffset + 1; index++)
            {
                if (ranks[index].IdKey == 0)
                {
                    continue;
                }
                okRanks.Add(ranks[index]);
            }
            // Allow some variance

            // Pick a quality...

            LootRank chosenRank = okRanks[_rand.Next() % okRanks.Count];

            ItemType itemType = null;

            if (lootGenData.ItemTypeId > 0)
            {
                itemType = _gameData.Get<ItemTypeSettings>(_gs.ch).Get(lootGenData.ItemTypeId);
            }
            if (itemType == null)
            {
                List<EquipSlot> okEquipSlots = _gameData.Get<EquipSlotSettings>(null).GetData().Where(x => x.IsCrawlerSlot).ToList();

                List<long> okEquipSlotIds = okEquipSlots.Select(x => x.IdKey).ToList();

                IReadOnlyList<ItemType> allLootItems = _gameData.Get<ItemTypeSettings>(null).GetData();

                List<ItemType> okLootItems = allLootItems.Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

                List<ItemType> weaponItems = okLootItems.Where(x => EquipSlots.IsWeapon(x.EquipSlotId)).ToList();

                List<ItemType> armorItems = okLootItems.Where(x => EquipSlots.IsArmor(x.EquipSlotId)).ToList();

                bool armorItem = _rand.NextDouble() < rankSettings.ArmorChance;

                List<ItemType> finalList = (armorItem ? armorItems : weaponItems);

                if (finalList.Count < 1)
                {
                    return null;
                }

                itemType = finalList[_rand.Next() % finalList.Count];
            }

            bool isArmor = EquipSlots.IsArmor(itemType.EquipSlotId);

            ScalingType scalingType = null;
            long scalingTypeId = 0;

            if (isArmor)
            {
                scalingTypeId = MathUtils.IntRange(1, LootConstants.MaxArmorScalingType, _rand);
                scalingType = _gameData.Get<ScalingTypeSettings>(null).Get(scalingTypeId);
            }

            Item item = new Item() { Id = Guid.NewGuid().ToString() };

            item.ItemTypeId = itemType.IdKey;

            if (isArmor)
            {
                item.ScalingTypeId = scalingTypeId;
            }
            item.LootRankId = chosenRank.IdKey;
            item.QualityTypeId = 0;

            if (isArmor)
            {

                EquipSlot equipSlot = _gameData.Get<EquipSlotSettings>(null).Get(itemType.EquipSlotId);

                if (equipSlot == null || equipSlot.BaseBonusStatTypeId < 1)
                {
                    item.ScalingTypeId = 0;
                }
                else
                {
                    if (equipSlot.BaseBonusStatTypeId != StatTypes.Armor)
                    {
                        item.ScalingTypeId = 0;
                    }
                    long bonusStat = itemType.MinVal;
                    if (scalingType != null)
                    {
                        bonusStat = Math.Max(1, (bonusStat * scalingType.ArmorPct) / 100);
                    }
                    item.Effects.Add(new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = equipSlot.BaseBonusStatTypeId, Quantity = bonusStat });
                }
            }


            string baseItemName = itemType.Name;
            if (itemType.Names.Count > 0)
            {
                float weightSum = itemType.Names.Sum(x => x.Weight);
                double valChosen = _rand.NextDouble() * weightSum;
                foreach (WeightedName wn in itemType.Names)
                {
                    valChosen -= wn.Weight;
                    if (valChosen <= 0)
                    {
                        baseItemName = wn.Name;
                        break;
                    }
                }
            }
            // Weapon damage is calculated dynamically as needed.


            if (itemType.EquipSlotId == EquipSlots.Quiver || itemType.EquipSlotId == EquipSlots.PoisonVial)
            {
                List<ElementType> okElements = _gameData.Get<ElementTypeSettings>(null).GetData().Where(x => x.IdKey > 1).ToList();

                ElementType okElement = okElements[_rand.Next() % okElements.Count];

                ItemProc iproc = new ItemProc()
                {
                    EntityTypeId = EntityTypes.Damage,
                    EntityId = 0,
                    ElementTypeId = okElement.IdKey,
                    PercentChance = 50,
                    MinQuantity = level / 5,
                    MaxQuantity = level / 2,
                };
                item.Procs.Add(iproc);
                if (itemType.EquipSlotId == EquipSlots.Quiver)
                {
                    item.Name = chosenRank.Name + " " + okElement.Name + " Quiver";
                }
                else if (itemType.EquipSlotId == EquipSlots.PoisonVial)
                {
                    item.Name = chosenRank.Name + " Vial of " + okElement.Name;
                }
            }
            else
            {
                List<StatType> okStats = _gameData.Get<StatSettings>(null).GetData()
                    .Where(x => x.IdKey >= StatConstants.PrimaryStatStart &&
                x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

                StatType okStat = okStats[_rand.Next() % okStats.Count];

                long statTypeId = okStat.IdKey;

                ItemEffect itemEffect = new ItemEffect()
                {
                    EntityTypeId = EntityTypes.Stat,
                    EntityId = statTypeId,
                    Quantity = 1 + level / 10,
                };

                item.Effects.Add(itemEffect);

                item.Name = chosenRank.Name + " " + baseItemName + " of " + okStat.Name;
                item.Level = level;

            }

            double cost = lootSettings.BaseLootCost;

            cost = cost * (1 + (itemType.MinVal + itemType.MaxVal) / 2.0f);

            if (itemType.EquipSlotId == EquipSlots.MainHand)
            {
                cost *= lootSettings.WeaponMult;
                if (itemType.HasFlag(ItemFlags.FlagTwoHandedItem))
                {
                    cost *= lootSettings.TwoHandWeaponMult;
                }
            }

            if (item.Procs.Count > 0)
            {
                cost *= lootSettings.ProcMult;
            }
            if (item.Effects.Count > 0)
            {
                cost *= lootSettings.EffectMult;
            }

            if (isArmor)
            {
                cost = cost * scalingType.CostPct / 100.0f;
            }

            cost = cost * chosenRank.CostPct / 100.0f;

            item.BuyCost = (long)cost;
            item.SellValue = (long)(cost * _gameData.Get<VendorSettings>(_gs.ch).SellToVendorPriceMult);
            return item;
        }

        public PartyLoot GiveCombatLoot(PartyData party, CrawlerCombatState combatState, CrawlerMap map)
        {

            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(null);

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            double itemChance = lootSettings.ItemChancePerMonster;

            long exp = 0;
            long gold = 0;

            int itemCount = 0;

            long minGold = 5 + (long)(party.Combat.Level * lootSettings.MinGoldPerKillLevelMult);
            long maxGold = (long)(minGold * lootSettings.MaxGoldPerKillLevelMult);
            foreach (CrawlerUnit crawlerUnit in party.Combat.EnemiesKilled)
            {
                exp += trainingSettings.GetMonsterExp(party.Combat.Level);
                gold += MathUtils.LongRange(minGold, maxGold, _rand);

                if (_rand.NextDouble() < itemChance)
                {
                    itemCount++;
                }
            }

            if (itemCount < 1 && _rand.NextDouble() < itemChance * 2)
            {
                itemCount++;
            }

            LootGenData allLootGenData = new LootGenData()
            {
                Gold = gold,
                Exp = exp,
                Level = party.Combat.Level,
                ItemCount = itemCount,
            };

            return GiveLoot(party, map, allLootGenData);
        }

        public PartyLoot GiveLoot(PartyData party, CrawlerMap map, LootGenData genData)
        {
            PartyLoot loot = new PartyLoot();
            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            List<Item> items = new List<Item>();

            ItemGenData itemGenData = new ItemGenData()
            {
                Level = genData.Level,
            };

            for (int i = 0; i < genData.ItemCount; i++)
            {
                Item item = GenerateItem(itemGenData);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            List<long> questItems = map.Details.Where(x => x.EntityTypeId == EntityTypes.QuestItem && x.EntityId > 0).Select(x => x.EntityId).ToList();

            foreach (long questItemId in questItems)
            {
                loot.NewQuestItems.Add(questItemId);
                PartyQuestItem pqi = party.QuestItems.FirstOrDefault(x => x.CrawlerQuestItemId == questItemId);
                if (pqi == null)
                {
                    pqi = new PartyQuestItem() { CrawlerQuestItemId = questItemId };
                    party.QuestItems.Add(pqi);
                }
                pqi.Quantity++;
            }

            items = items.OrderByDescending(x => x.BuyCost).ToList();

            while (items.Count > lootSettings.MaxLootItems)
            {
                Item lastItem = items.Last();
                items.Remove(lastItem);

                genData.Gold += lastItem.BuyCost;
            }

            loot.Items = items;

            party.Gold += genData.Gold;

            loot.Gold = genData.Gold;

            List<PartyMember> activeMembers = party.GetActiveParty()
                .Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

            int aliveCount = activeMembers.Count;

            if (aliveCount < 1)
            {
                return new PartyLoot();
            }
            loot.Exp = genData.Exp / aliveCount;

            foreach (PartyMember member in activeMembers)
            {
                member.Exp += loot.Exp;
            }

            party.Inventory.AddRange(loot.Items);

            return loot;
        }


        List<long> okEquipSlotIds = new List<long>() { EquipSlots.Necklace, EquipSlots.Ring1, EquipSlots.Jewelry1, EquipSlots.OffHand };
        public List<string> GenerateItemNames(IRandom rand, int itemCount)
        {
            List<ItemType> okItemTypes = _gameData.Get<ItemTypeSettings>(null).GetData().Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

            okItemTypes = okItemTypes.Where(x => x.Name != "Shield").ToList();

            List<string> retval = new List<string>();

            for (int i = 0; i < itemCount; i++)
            {
                long lootQualityId = QualityTypes.Legendary;

                long itemTypeId = okItemTypes[rand.Next() % okItemTypes.Count].IdKey;

                retval.Add(_itemGenService.GenerateName(rand, itemTypeId, 100, lootQualityId, new List<FullReagent>()));
            }

            return retval;
        }

    }
}
