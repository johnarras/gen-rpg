using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class CombatLootStateHelper : BaseCombatStateHelper
    {

        private ILootGenService _lootService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.CombatLoot; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.Actions.Add(new CrawlerStateAction("\nYou are victorious! You receive!\n"));

            PartyData party = _crawlerService.GetParty();

            CombatLoot loot = _lootService.GiveLoot(gs, party);

            if (loot.Exp > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction(loot.Exp + " Exp  per surviving party member!"));
            }
            if (loot.Gold > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction(loot.Gold + " Gold!"));
            }

            foreach (Item item in loot.Items)
            {
                stateData.Actions.Add(new CrawlerStateAction(item.Name + "!"));
            }

            stateData.Actions.Add(new CrawlerStateAction($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...", KeyCode.Space, ECrawlerStates.ExploreWorld));


            party.Combat = null;

            await _crawlerService.SaveGame();

            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
