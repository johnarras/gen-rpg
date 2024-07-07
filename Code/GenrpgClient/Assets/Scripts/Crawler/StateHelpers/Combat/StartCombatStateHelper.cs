using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class StartCombatStateHelper : BaseCombatStateHelper
    {
        private IScreenService _screenService;
        private ICrawlerWorldService _crawlerWorldService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.StartCombat; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = null;

            PartyData party = _crawlerService.GetParty();

            InitialCombatState initialState = action.ExtraData as InitialCombatState;

            if (initialState == null)
            {
                initialState = new InitialCombatState();
            }

            if (await _combatService.StartCombat(_crawlerService.GetParty(), initialState))
            {
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun,true);
            }
            else
            {
                stateData = new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Failed to start combat." };
            }

            await Task.CompletedTask;
            return stateData;
        }
    }
}
