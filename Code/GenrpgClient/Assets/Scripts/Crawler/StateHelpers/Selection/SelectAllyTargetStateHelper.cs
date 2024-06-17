using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Selection
{
    public class SelectAllyTargetStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectAllyTarget; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();

            SelectSpellAction selectSpellAction = action.ExtraData as SelectSpellAction;

            SelectAction selectAction = null;

            if (selectSpellAction != null)
            {
                selectAction = selectSpellAction.Action;
            }
            else
            {
                selectAction = action.ExtraData as SelectAction;
            }

            if (selectAction == null || 
                selectAction.Action == null ||
                selectAction.Member == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ErrorMessage = "Cannot select ally without select action" };
            }

            List<PartyMember> partyMembers = party.GetActiveParty();

            bool selectingCaster = false;
            ECrawlerStates nextAction = ECrawlerStates.SelectSpell;
            if (selectAction.Member == null)
            {
                partyMembers = partyMembers.Where(x => !_combatService.IsDisabled(x)).ToList();
                selectingCaster = true;
            }
            else
            {
                nextAction = selectAction.NextState;
            }

            
            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)('a' + m);

                Action clickAction = null;

                if (selectingCaster)
                {
                    clickAction = delegate ()
                    {
                        selectAction.Member = partyMember;

                    };
                }
                else // Selecting target.
                {
                    clickAction = delegate ()
                    {
                        selectAction.Action.FinalTargets.Add(partyMember);
                        selectAction.Member.Action = selectAction.Action;
                    };
                }

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + partyMember.Name, (KeyCode)c,
                  nextAction, clickAction, action.ExtraData));
            }

            stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, selectAction.ReturnState));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
