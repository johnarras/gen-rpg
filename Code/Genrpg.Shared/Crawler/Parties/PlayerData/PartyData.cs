using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crawler.Combat.Entities;
using Newtonsoft.Json;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Constants;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    /// <summary>
    /// Used to contain a list of party members
    /// </summary>

    [MessagePackObject]
    public class PartyData : NoChildPlayerData, IUserData, INamedUpdateData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<PartyMember> Members { get; set; } = new List<PartyMember>();

        [Key(2)] public string Name { get; set; }
        [Key(3)] public DateTime UpdateTime { get; set; }

        [JsonIgnore]
        [Key(4)] public List<Item> Inventory { get; set; } = new List<Item> ();

        [Key(5)] public List<CrawlerSaveItem> SaveInventory { get; set; } = new List<CrawlerSaveItem>();

        [JsonIgnore]
        [Key(6)] public List<Item> VendorBuyback { get; set; } = new List<Item>();

        [JsonIgnore]
        [Key(7)] public List<Item> VendorItems { get; set; } = new List<Item>();

        [Key(8)] public List<PartyQuestItem> QuestItems { get; set; } = new List<PartyQuestItem>();

        [Key(9)] public DateTime LastVendorRefresh { get; set; }

        [Key(10)] public long Gold { get; set; } = 0;

        [Key(11)] public long Seed { get; set; }

        [Key(12)] public long WorldId { get; set; }

        [Key(13)] public long MapId { get; set; }

        [Key(14)] public int MapX { get; set; }
        [Key(15)] public int MapZ { get; set; }
        [Key(16)] public int MapRot { get; set; }

        [Key(17)] public long NextGroupId { get; set; }

        [Key(18)] public long NextItemId { get; set; }

        [Key(19)] public List<CrawlerMapStatus> Maps { get; set; } = new List<CrawlerMapStatus>();

        [Key(20)] public CurrentMapStatus CurrentMap { get; set; } = new CurrentMapStatus();

        [Key(21)] public SmallIndexBitList CompletedMaps { get; set; } = new SmallIndexBitList();

        [Key(22)] public float HourOfDay { get; set; } = 0;

        [Key(23)] public long DaysPlayed { get; set; } = 0;

        [Key(24)] public bool InGuildHall { get; set; }

        [Key(25)] public ECrawlerGameModes GameMode { get; set; }

        [Key(26)] public int MaxLevel { get; set; }
        [Key(27)] public long UpgradePoints { get; set; }

        [Key(28)] public List<PartyRoguelikeUpgrade> RoguelikeUpgrades { get; set; } = new List<PartyRoguelikeUpgrade>();

        [Key(29)] public long SaveSlotId { get; set; }

        [Key(30)] public float CombatTextScrollDelay { get; set; } = CrawlerCombatConstants.MaxCombatTextScrollDelay;

        [Key(31)] public SmallIdFloatCollection Buffs { get; set; } = new SmallIdFloatCollection();

        [JsonIgnore] public CrawlerCombatState Combat = null;

        public string GetNextGroupId()
        {
            return (++NextGroupId).ToString();
        }

        public string GetNextItemId()
        {
            return (++NextItemId).ToString();
        }

        public PartyMember GetMemberInSlot(int slot)
        {
            return Members.FirstOrDefault(x => x.PartySlot == slot);
        }
     
        public List<PartyMember> GetActiveParty()
        {

            return Members.Where(x => x.PartySlot > 0).ToList();
        }

        public EActionCategories GetActionCategory()
        {
            if (Combat == null)
            {
                return EActionCategories.NonCombat;
            }
            if (Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Prepare)
            {
                return EActionCategories.Preparing;
            }
            return EActionCategories.Combat;
        }

        public long GetUpgradeTier(long roguelikeUpgradeId)
        {

            if (GameMode != ECrawlerGameModes.Roguelite)
            {
                return 0;
            }

            PartyRoguelikeUpgrade upgrade = RoguelikeUpgrades.FirstOrDefault(x=>x.RoguelikeUpgradeId == roguelikeUpgradeId);
            if (upgrade == null)
            {
                return 0;
            }
            return upgrade.Tier;
        }

        public void SetUpgradeTier(long roguelikeUpgradeId, long newTier)
        {

            if (GameMode != ECrawlerGameModes.Roguelite)
            {
                return;
            }

            PartyRoguelikeUpgrade upgrade = RoguelikeUpgrades.FirstOrDefault(x => x.RoguelikeUpgradeId == roguelikeUpgradeId);
            if (upgrade == null)
            {
                upgrade = new PartyRoguelikeUpgrade() { RoguelikeUpgradeId = roguelikeUpgradeId, Tier = 0 };
                RoguelikeUpgrades.Add(upgrade);
            }

            upgrade.Tier = newTier;
        }
    }

    [MessagePackObject]
    public class PartyDataLoader : UnitDataLoader<PartyData>
    {
    }

    [MessagePackObject]
    public class PartyDataMapper : UnitDataMapper<PartyData> { }
}
