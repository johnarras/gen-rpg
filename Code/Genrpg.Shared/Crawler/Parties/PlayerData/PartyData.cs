using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Casting;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Crawler.Parties.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Newtonsoft.Json;
using Genrpg.Shared.Crawler.UI.Interfaces;
using Newtonsoft.Json.Serialization;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.Crawler.Combat.Constants;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    /// <summary>
    /// Used to contain a list of party members
    /// </summary>

    [MessagePackObject]
    public class PartyData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<PartyMember> Members { get; set; } = new List<PartyMember>();

        [JsonIgnore]
        [Key(2)] public List<Item> Inventory { get; set; } = new List<Item> ();

        [Key(3)] public List<CrawlerSaveItem> SaveInventory { get; set; } = new List<CrawlerSaveItem>();

        [JsonIgnore]
        [Key(4)] public List<Item> VendorBuyback { get; set; } = new List<Item>();

        [JsonIgnore]
        [Key(5)] public List<Item> VendorItems { get; set; } = new List<Item>();

        [Key(6)] public List<PartyQuestItem> QuestItems { get; set; } = new List<PartyQuestItem>();

        [Key(7)] public DateTime LastVendorRefresh { get; set; }

        [Key(8)] public long Gold { get; set; } = 0;

        [Key(9)] public long Seed { get; set; }

        [Key(10)] public long WorldId { get; set; }

        [Key(11)] public long MapId { get; set; }

        [Key(12)] public int MapX { get; set; }
        [Key(13)] public int MapZ { get; set; }
        [Key(14)] public int MapRot { get; set; }

        [Key(15)] public long NextItemId { get; set; }

        [Key(16)] public List<CrawlerMapStatus> Maps { get; set; } = new List<CrawlerMapStatus>();

        [Key(17)] public CrawlerMapStatus CurrentMap { get; set; } = new CrawlerMapStatus();

        [Key(18)] public SmallIndexBitList CompletedMaps { get; set; } = new SmallIndexBitList();

        [Key(19)] public double HourOfDay { get; set; } = 0;

        [Key(20)] public long DaysPlayed { get; set; } = 0;

        [Key(21)] public bool InTavern { get; set; }

        [JsonIgnore] public IWorldPanel WorldPanel = null;
        [JsonIgnore] public IActionPanel ActionPanel = null;
        [JsonIgnore] public IStatusPanel StatusPanel = null;
        [JsonIgnore] public ISpeedupListener SpeedupListener = null;
        [JsonIgnore] public CrawlerCombatState Combat = null;

        public PartyMember GetMemberInSlot(int slot)
        {
            if (slot >= 1 && slot <= PartyConstants.MaxPartySize)
            {
                return Members.FirstOrDefault(x=>x.PartySlot == slot);
            }
            return null;
        }

        public void AddPartyMember(PartyMember member)
        {
            for (int i = 1; i <= PartyConstants.MaxPartySize; i++)
            {
                if (GetMemberInSlot(i) == null)
                {
                    member.PartySlot = i;
                }
            }

            FixPartySlots();
        }

        public void RemovePartyMember(PartyMember member)
        {
            member.PartySlot = 0;
            FixPartySlots();
        }

        public void DeletePartyMember(PartyMember member)
        {
            if (member.PartySlot > 0)
            {
                return;
            }
            Members.Remove(member);
            FixPartySlots();
        }

        public void FixPartySlots()
        {
            List<PartyMember> currentMembers = Members.Where(x => x.PartySlot > 0).OrderBy(x => x.PartySlot).ToList();

            for (int i = 0; i< currentMembers.Count; i++)
            {
                if (i < PartyConstants.MaxPartySize)
                {
                    currentMembers[i].PartySlot = i + 1;
                }
                else
                {
                    currentMembers[i].PartySlot = 0;
                }
            }

        }

        public List<PartyMember> GetActiveParty()
        {
            List<PartyMember> retval = new List<PartyMember>();

            for (int i = 1; i <= PartyConstants.MaxPartySize; i++)
            {
                PartyMember member = GetMemberInSlot(i);
                if (member != null)
                {
                    retval.Add(member);
                }
            }
            return retval;
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
    }

    [MessagePackObject]
    public class PartyDataLoader : UnitDataLoader<PartyData>
    {
    }

    [MessagePackObject]
    public class PartyDataMapper : UnitDataMapper<PartyData> { }
}
