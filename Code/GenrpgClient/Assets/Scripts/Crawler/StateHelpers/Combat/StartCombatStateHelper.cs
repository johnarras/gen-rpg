using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UI.Screens.Constants;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class StartCombatStateHelper : BaseCombatStateHelper
    {
        private IScreenService _screenService;
        

        public override ECrawlerStates GetKey() { return ECrawlerStates.StartCombat; }

        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = null;

            PartyData party = _crawlerService.GetParty();

            CombatState combatState = new CombatState() { Level = party.GetWorldLevel() };


            if (_combatService.StartCombat(gs, _crawlerService.GetParty(), combatState))
            {
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun,true);
            }
            else
            {
                stateData = new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Failed to start combat." };
            }

            await UniTask.CompletedTask;
            return stateData;
        }
    }
}
