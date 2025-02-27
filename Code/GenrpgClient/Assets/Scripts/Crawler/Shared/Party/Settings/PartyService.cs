using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Roguelikes.Constants;
using Genrpg.Shared.Crawler.Roguelikes.Services;
using Genrpg.Shared.Crawler.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Party.Services
{

    public interface IPartyService : IInjectable
    {
        long GetMaxPartySize(PartyData partyData);
        void AddPartyMember(PartyData partyData, PartyMember member);
        void RemovePartyMember(PartyData partyData, PartyMember member);
        void DeletePartyMember(PartyData partyData, PartyMember member);
        void Reset(PartyData partyData);

    }
    
    public class PartyService : IPartyService
    {
        private IGameData _gameData;
        private IClientGameState _gs;
        private IClientRandom _rand;
        private IRoguelikeUpgradeService _upgradeService;
        private IDispatcher _dispatcher;


        public long GetMaxPartySize(PartyData partyData)
        {
            CrawlerSettings settings = _gameData.Get<CrawlerSettings>(_gs.ch);

            if (partyData.GameMode == ECrawlerGameModes.Roguelite)
            {
                return settings.RoguelikePartySize + (int)_upgradeService.GetBonus(partyData, RoguelikeUpgrades.PartySize);
            }
            else
            {
                return settings.CrawlerPartySize;
            }
        }

        public void AddPartyMember(PartyData partyData, PartyMember member)
        {
            for (int i = 1; i <= GetMaxPartySize(partyData); i++)
            {
                if (partyData.GetMemberInSlot(i) == null)
                {
                    member.PartySlot = i;
                }
            }

            FixPartySlots(partyData);
        }

        public void RemovePartyMember(PartyData partyData, PartyMember member)
        {
            member.PartySlot = 0;
            FixPartySlots(partyData);
        }

        public void DeletePartyMember(PartyData partyData, PartyMember member)
        {
            if (member.PartySlot > 0)
            {
                return;
            }
            partyData.Members.Remove(member);
            if (partyData.GameMode == ECrawlerGameModes.Roguelite && partyData.Members.Count < 1)
            {
                partyData.Gold = 0;
                partyData.Inventory = new List<Item>();
                partyData.VendorBuyback = new List<Item>();
                partyData.VendorItems = new List<Item>();
            }
            FixPartySlots(partyData);
        }

        public void FixPartySlots(PartyData partyData)
        {
            List<PartyMember> currentMembers = partyData.Members.Where(x => x.PartySlot > 0).OrderBy(x => x.PartySlot).ToList();

            for (int i = 0; i < currentMembers.Count; i++)
            {
                if (i < GetMaxPartySize(partyData))
                {
                    currentMembers[i].PartySlot = i + 1;
                }
                else
                {
                    currentMembers[i].PartySlot = 0;
                }
            }
            _dispatcher.Dispatch(new RefreshPartyStatus());
        }


        public void Reset(PartyData partyData)
        {
            if (partyData.WorldId == 0)
            {
                partyData.WorldId = _rand.Next() % 100000000;
            }
            partyData.Maps = new List<CrawlerMapStatus>();
            partyData.CurrentMap = new CurrentMapStatus();
            partyData.LastVendorRefresh = DateTime.UtcNow.AddDays(-1);
            partyData.Inventory = new List<Item>();
            partyData.VendorBuyback = new List<Item>();
            partyData.VendorItems = new List<Item>();

            partyData.MapId = 0;
            partyData.MapX = -1;
            partyData.MapZ = -1;
            partyData.InGuildHall = true;
            partyData.DaysPlayed = 0;
            if (partyData.GameMode == ECrawlerGameModes.Roguelite)
            {
                partyData.Members.Clear();
                partyData.MaxLevel = 0;
            }
            else
            {
                foreach (PartyMember member in partyData.Members)
                {
                    member.StatusEffects.Clear();
                }
                partyData.RoguelikeUpgrades = new List<PartyRoguelikeUpgrade>();
                partyData.MaxLevel = 0;
            }

            partyData.Gold = 0;
            partyData.HourOfDay = 0;
            partyData.CompletedMaps.Clear();
        }
    }
}
