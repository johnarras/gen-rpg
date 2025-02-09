
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Combat;
using Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection
{
    public class SelectEnemyGroupStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectEnemyGroup; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            if (party.Combat == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Cannot select enemies out of combat" };
            }

            SelectSpellAction selectAction = action.ExtraData as SelectSpellAction;

            if (selectAction == null || selectAction.Action == null || selectAction.Action.Action == null ||
                selectAction.Action.Member == null ||
                selectAction.Action.Action.PossibleTargetGroups.Count < 1)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "No enemy groups to select" };
            }

            CrawlerUnit currUnit = selectAction.Action.Member;

            for (int m = 0; m < selectAction.Action.Action.PossibleTargetGroups.Count; m++)
            {
                CombatGroup group = selectAction.Action.Action.PossibleTargetGroups[m];
                char c = (char)('a' + m);

                Action clickRowAction = delegate ()
                { 
                    selectAction.Action.Action.FinalTargets = group.Units.ToList();
                    currUnit.Action = selectAction.Action.Action;
                    selectAction.Action.Action.FinalTargetGroups = new List<CombatGroup>() { group };
                    _dispatcher.Dispatch(new ClearCombatGroupActions());
                };

                CrawlerStateAction newAction = new CrawlerStateAction(char.ToUpper(c) + " " + group.ShowStatus(), c,
                    selectAction.Action.NextState, onClickAction: clickRowAction, forceButton: false);

                stateData.Actions.Add(newAction);

                Action clickIconAction =
                    delegate
                    {
                        _crawlerService.ChangeState(stateData, newAction, _crawlerService.GetToken());
                    };


                _dispatcher.Dispatch(new SetCombatGroupAction() { Action = clickIconAction, Group = group });

            }

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.CombatPlayer,
                delegate ()
                {
                    _dispatcher.Dispatch(new ClearCombatGroupActions());
                }));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
