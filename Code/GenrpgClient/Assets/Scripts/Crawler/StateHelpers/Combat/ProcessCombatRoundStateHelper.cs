using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.Services.Combat;
using Assets.Scripts.Crawler.UI.Utils;
using Assets.Scripts.UI.Crawler.States;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Units.Loaders;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

namespace Assets.Scripts.Crawler.StateHelpers.Combat
{
    public class ProcessCombatRoundStateHelper : BaseCombatStateHelper
    {

        private IProcessCombatRoundCombatService _processCombatService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ProcessCombatRound; }
        
        public override async UniTask<CrawlerStateData> Init(UnityGameState gs, CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (!_combatService.ReadyForCombat(gs, party))
            {
                _combatService.EndCombatRound(gs, party);
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }

            ProcessCombat(gs, party, token).Forget();

            await UniTask.CompletedTask;
            return stateData;
        }

        private async UniTask ProcessCombat(UnityGameState gs, PartyData party, CancellationToken token)
        {

            await UniTask.Delay(100, cancellationToken: token);
            bool success = await _processCombatService.ProcessCombatRound(gs, party, token);

            party.ActionPanel.AddText($"\n\nPress {CrawlerUIUtils.HighlightText("Space")} to continue...\n\n");
            
            for (int i = 0; i < 1; i++)
            {
                await UniTask.NextFrame(token);
                party.ActionPanel.AddText("\n");
            }

            while (!party.SpeedupListener.TriggerSpeedupNow())
            {
                await UniTask.NextFrame(token);
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
