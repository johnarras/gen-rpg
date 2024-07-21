using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;

using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Spells.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Selection
{
    public class SelectEnemyGroupStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectEnemyGroup; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
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

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + group.ShowStatus(), (KeyCode)c,
                    selectAction.Action.NextState, onClickAction: delegate ()
                    {
                        selectAction.Action.Action.FinalTargets = group.Units.ToList();
                        currUnit.Action = selectAction.Action.Action;
                        selectAction.Action.Action.FinalTargetGroups = new List<CombatGroup>() { group };
                    }));

            }

            stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, ECrawlerStates.CombatPlayer));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
