﻿using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Combat
{
    public class ProcessCombatRoundStateHelper : BaseCombatStateHelper
    {

        private IProcessCombatRoundCombatService _processCombatService;

        public override ECrawlerStates GetKey() { return ECrawlerStates.ProcessCombatRound; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (!_combatService.ReadyForCombat(party))
            {
                await _combatService.EndCombatRound(party);
                stateData = new CrawlerStateData(ECrawlerStates.CombatFightRun, true);
            }

            _ = Task.Run(() => ProcessCombat(party, token));


            await Task.CompletedTask;
            return stateData;
        }

        private bool _canContinueCombat = false;
        private async Task ProcessCombat(PartyData party, CancellationToken token)
        {
            _canContinueCombat = false;
            await Task.Delay(100, token);
            bool success = await _processCombatService.ProcessCombatRound(party, token);

            party.ActionPanel.AddText($"\n\nPress {_textService.HighlightText("Space")} to continue...\n\n",
                () => { 
                    _canContinueCombat = true;
                    _logService.Info("Did Press Space");
                });

            for (int i = 0; i < 1; i++)
            {
                await Task.Delay(10, token);
                party.ActionPanel.AddText("\n");
            }

            while (!party.SpeedupListener.TriggerSpeedupNow() && !_canContinueCombat)
            {
                await Task.Delay(10, token);
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
                    _crawlerService.ChangeState(ECrawlerStates.GiveLoot, token, party.Combat);
                }
            }
        }
    }
}