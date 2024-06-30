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

        public override ECrawlerStates GetKey() { return ECrawlerStates.StartCombat; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = null;

            PartyData party = _crawlerService.GetParty();

            CombatState combatState = new CombatState() { Level = await _worldService.GetMapLevelAtParty(party) };

            if (await _combatService.StartCombat(_crawlerService.GetParty(), combatState))
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
