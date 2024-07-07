using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.StateHelpers;

using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Crawler.States
{
    public class TavernMainHelper : BaseStateHelper
    {

        private ILootGenService _lootGenService;
        private ICrawlerWorldService _crawlerWorldService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.TavernMain; }


        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.WorldSpriteName = CrawlerClientConstants.TavernImage;

            PartyData party = _crawlerService.GetParty();

            foreach (PartyMember member in party.Members)
            {
                member.Stats.SetCurr(StatTypes.Health, member.Stats.Max(StatTypes.Health));
                member.Stats.SetCurr(StatTypes.Mana, member.Stats.Max(StatTypes.Mana));
                member.StatusEffects.Clear();
            }

            string txt = action.ExtraData as String;

            if (txt != null && txt == "GenerateWorld")
            {
                await _worldService.GenerateWorld(party); 
            }

            stateData.Actions.Add(new CrawlerStateAction("Add Party Member", KeyCode.A, ECrawlerStates.AddMember));
            stateData.Actions.Add(new CrawlerStateAction("Remove Party Member", KeyCode.R, ECrawlerStates.RemoveMember));
            stateData.Actions.Add(new CrawlerStateAction("Delete Party Member", KeyCode.D, ECrawlerStates.DeleteMember));
            stateData.Actions.Add(new CrawlerStateAction("Create Party Member", KeyCode.C, ECrawlerStates.ChooseSex));
            stateData.Actions.Add(new CrawlerStateAction("New Game", KeyCode.N, ECrawlerStates.TavernMain, null, "GenerateWorld"));
            if (party.GetActiveParty().Count > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Enter Map", KeyCode.E, ECrawlerStates.ExploreWorld));
            }
            stateData.Actions.Add(new CrawlerStateAction("More Options", KeyCode.M, ECrawlerStates.Options));


            await Task.CompletedTask;
            return stateData;

        }
    }
}
