using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class ProcessCombatRoundStateHelper : BaseCombatStateHelper
    {

        private IProcessCombatRoundCombatService _processCombatService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ProcessCombatRound; }
        
        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (!_combatService.ReadyForCombat(party))
            {
                _combatService.EndCombatRound(party);
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }

            AwaitableUtils.ForgetAwaitable(ProcessCombat(party, token));


            await Task.CompletedTask;
            return stateData;
        }

        private async Awaitable ProcessCombat(PartyData party, CancellationToken token)
        {

            await Awaitable.WaitForSecondsAsync(0.1f, cancellationToken: token);
            bool success = await _processCombatService.ProcessCombatRound(party, token);

            party.ActionPanel.AddText($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...\n\n");
            
            for (int i = 0; i < 1; i++)
            {
                await Awaitable.NextFrameAsync(token);
                party.ActionPanel.AddText("\n");
            }

            while (!party.SpeedupListener.TriggerSpeedupNow())
            {
                await Awaitable.NextFrameAsync(token);
            }

            if (!success || party.Combat == null)
            {
                _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token);
            }
            else
            {
                if (!party.Combat.CombatIsOver())
                {
                    _crawlerService.ChangeState(ECrawlerStates.CombatFightRun, token);
                }
                else
                {
                    _crawlerService.ChangeState(ECrawlerStates.CombatLoot, token);
                }
            }               
        }
    }
}
