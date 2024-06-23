using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.StateHelpers.Training
{
    public class TrainingMainHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() { return ECrawlerStates.TrainingMain; }

        public override async Awaitable<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.WorldSpriteName = CrawlerClientConstants.TrainerImage;
            PartyData party = _crawlerService.GetParty();

            foreach (PartyMember member in party.GetActiveParty())
            {

                ECrawlerStates nextState = ECrawlerStates.TrainingLevel;
                KeyCode nextKeyCode = (KeyCode)(member.PartySlot + '0');
                if (_combatService.IsDisabled(member))
                {
                    nextState = ECrawlerStates.None;
                    nextKeyCode = KeyCode.None;
                }

                stateData.Actions.Add(new CrawlerStateAction(member.PartySlot + " " + member.Name, nextKeyCode, nextState, extraData: member));
            }




            stateData.Actions.Add(new CrawlerStateAction("Back to the city", KeyCode.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
