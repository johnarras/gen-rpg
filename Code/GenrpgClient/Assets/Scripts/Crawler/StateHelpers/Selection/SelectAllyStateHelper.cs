using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.Crawler.StateHelpers.Combat;
using Assets.Scripts.Crawler.StateHelpers.Selection.Entities;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Selection
{
    public class SelectAllyStateHelper : BaseCombatStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.SelectAlly; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            PartyData party = _crawlerService.GetParty();
            List<PartyMember> partyMembers = party.GetActiveParty();

            SelectSpellAction spellAction = new SelectSpellAction();
            
            for (int m = 0; m < partyMembers.Count; m++)
            {
                PartyMember partyMember = partyMembers[m];
                char c = (char)('a' + m);

                SelectAction selectAction = new SelectAction()
                {
                    Member = partyMember,
                    ReturnState = ECrawlerStates.SelectAlly,
                    NextState = ECrawlerStates.WorldCast,
                };

                stateData.Actions.Add(new CrawlerStateAction(char.ToUpper(c) + " " + partyMember.Name, (KeyCode)c,
                  ECrawlerStates.SelectSpell, extraData: selectAction));
            }

            stateData.Actions.Add(new CrawlerStateAction("", KeyCode.Escape, ECrawlerStates.ExploreWorld));


            await Task.CompletedTask;
            return stateData;
        }
    }
}
