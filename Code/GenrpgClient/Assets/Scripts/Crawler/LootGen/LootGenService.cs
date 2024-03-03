using Assets.Scripts.Crawler.Services.Combat;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Crawler.Loot.Constants;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.PlayerData;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Loot.Services
{
    public class LootGenData
    {
        public long Level { get; set; }
    }


    public class LootGenService : ILootGenService
    {
        public async Task Setup(GameState gs, CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public Item GenerateItem(GameState gs, LootGenData lootGenData)
        {

            return GenerateEquipment(gs, lootGenData);
        }

        public Item GenerateEquipment(GameState gs, LootGenData lootGenData)
        {
            long level = lootGenData.Level;

            CrawlerLootSettings lootSettings = gs.data.Get<CrawlerLootSettings>(null);

            LootRankSettings rankSettings = gs.data.Get<LootRankSettings>(null);

            IReadOnlyList<LootRank> ranks = rankSettings.GetData();

            int expectedOffset = (int)(level / rankSettings.LevelsPerQuality + 1);

            expectedOffset = MathUtils.Clamp(1, expectedOffset, ranks.Count - 2);

            List<LootRank> okRanks = new List<LootRank>();

            while (expectedOffset < ranks.Count - 2 && gs.rand.NextDouble() < rankSettings.ExtraQualityChance)
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

            LootRank chosenRank = okRanks[gs.rand.Next() % okRanks.Count];

            IReadOnlyList<ItemType> allLootItems = gs.data.Get<ItemTypeSettings>(null).GetData();

            List<ItemType> okLootItems = allLootItems.Where(x => x.EquipSlotId > 0).ToList();

            List<ItemType> weaponItems = okLootItems.Where(x => EquipSlots.IsWeapon(x.EquipSlotId)).ToList();

            List<ItemType> armorItems = okLootItems.Where(x => EquipSlots.IsArmor(x.EquipSlotId)).ToList();

            bool isArmor = gs.rand.NextDouble() < rankSettings.ArmorChance;

            List<ItemType> finalList = (isArmor ? armorItems : weaponItems);

            if (finalList.Count < 1)
            {
                return null;
            }

            ItemType itemType = finalList[gs.rand.Next() % finalList.Count];

            ScalingType scalingType = null;
            long scalingTypeId = 0;

            if (finalList == armorItems)
            {
                scalingTypeId = MathUtils.IntRange(1, LootConstants.MaxArmorScalingType, gs.rand);
                scalingType = gs.data.Get<ScalingTypeSettings>(null).Get(scalingTypeId);
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

                EquipSlot equipSlot = gs.data.Get<EquipSlotSettings>(null).Get(itemType.EquipSlotId);

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
                double valChosen = gs.rand.NextDouble() * weightSum;
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
                List<ElementType> okElements = gs.data.Get<ElementTypeSettings>(null).GetData().Where(x => x.IdKey > 1).ToList();

                ElementType okElement = okElements[gs.rand.Next() % okElements.Count];

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
                List<StatType> okStats = gs.data.Get<StatSettings>(null).GetData()
                    .Where(x => x.IdKey >= StatConstants.PrimaryStatStart &&
                x.IdKey <= StatConstants.PrimaryStatEnd).ToList();

                StatType okStat = okStats[gs.rand.Next() % okStats.Count];

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

            item.Cost = (long)cost;
            return item;
        }

        public CombatLoot GiveLoot(GameState gs, PartyData party)
        {
            CombatLoot loot = new CombatLoot();
            if (party.Combat == null)
            {
                return loot;
            }
            List<PartyMember> activeMembers = party.GetActiveParty()
                .Where(x => !x.StatusEffects.HasBit(StatusEffects.Dead)).ToList();

            int aliveCount = activeMembers.Count;

            if (aliveCount < 1)
            {
                return loot;
            }

            CrawlerLootSettings lootSettings = gs.data.Get<CrawlerLootSettings>(null);

            CrawlerTrainingSettings trainingSettings = gs.data.Get<CrawlerTrainingSettings>(null);

            double itemChance = lootSettings.ItemChancePerMonster;

            long exp = 0;
            long gold = 0;

            long itemCount = 0;

            foreach (CrawlerUnit crawlerUnit in party.Combat.EnemiesKilled)
            {
                exp += trainingSettings.GetMonsterExp(party.Combat.Level);
                gold += MathUtils.LongRange(5+party.Combat.Level, 20+party.Combat.Level*5, gs.rand);

                if (gs.rand.NextDouble() < itemChance)
                {
                    itemCount++;
                }

            }

            List<Item> items = new List<Item>();

            LootGenData genData = new LootGenData()
            {
                Level = party.Combat.Level,
            };

            for (int i = 0; i < itemCount; i++)
            {
                Item item = GenerateItem(gs, genData);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            items = items.OrderByDescending(x => x.Cost).ToList();

            while (items.Count > lootSettings.MaxLootItems)
            {
                Item lastItem = items.Last();
                items.Remove(lastItem);

                gold += lastItem.Cost;
            }

            loot.Items = items;

            party.Gold += gold;

            loot.Gold = gold;
            

            exp /= aliveCount;
            loot.Exp = exp;

            foreach (PartyMember member in activeMembers)
            {
                member.Exp += exp;
            }

            party.Inventory.AddRange(loot.Items);

            return loot;
        }
    }
}
