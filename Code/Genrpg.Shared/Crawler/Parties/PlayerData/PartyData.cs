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

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    /// <summary>
    /// Used to contain a list of party members
    /// </summary>

    [MessagePackObject]
    public class PartyData : BasePlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<PartyMember> Members { get; set; } = new List<PartyMember>();

        [Key(2)] public List<Item> Inventory { get; set; } = new List<Item> ();

        [Key(3)] public List<Item> VendorBuyback { get; set; } = new List<Item>();

        [Key(4)] public List<Item> VendorItems { get; set; } = new List<Item>();

        [Key(5)] public DateTime LastVendorRefresh { get; set; }

        [Key(6)] public long Gold { get; set; } = 0;

        [Key(7)] public long Seed { get; set; }

        [Key(8)] public long MapId { get; set; }

        [Key(9)] public int MapX { get; set; }
        [Key(10)] public int MapZ { get; set; }
        [Key(11)] public int MapRot { get; set; }

        [JsonIgnore] public IWorldPanel WorldPanel = null;
        [JsonIgnore] public IActionPanel ActionPanel = null;
        [JsonIgnore] public IStatusPanel StatusPanel = null;
        [JsonIgnore] public ISpeedupListener SpeedupListener = null;
        [JsonIgnore] public CombatState Combat = null;

        public PartyMember GetMemberInSlot(int slot)
        {
            if (slot >= 1 && slot <= PartyConstants.PartySize)
            {
                return Members.FirstOrDefault(x=>x.PartySlot == slot);
            }
            return null;
        }

        public void AddPartyMember(PartyMember member)
        {
            for (int i = 1; i <= PartyConstants.PartySize; i++)
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
                if (i < PartyConstants.PartySize)
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

            for (int i = 1; i <= PartyConstants.PartySize; i++)
            {
                PartyMember member = GetMemberInSlot(i);
                if (member != null)
                {
                    retval.Add(member);
                }
            }
            return retval;
        }

        public int GetWorldLevel()
        {
            List<PartyMember> partyMembers = GetActiveParty();
            if (partyMembers.Count < 1)
            {
                return 1;
            }

            return (int)Math.Max(1, Math.Ceiling(1.0 * partyMembers.Sum(x => x.Level) / partyMembers.Count));
        }
    }

    [MessagePackObject]
    public class PartyDataLoader : UnitDataLoader<PartyData>
    {
        protected override bool IsUserData()
        {
            return true;
        }
    }
}
