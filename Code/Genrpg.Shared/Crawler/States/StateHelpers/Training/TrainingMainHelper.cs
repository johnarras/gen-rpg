using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;


namespace Genrpg.Shared.Crawler.States.StateHelpers.Training
{
    public class TrainingMainHelper : BuildingStateHelper
    {
       
        public override ECrawlerStates GetKey() { return ECrawlerStates.TrainingMain; }
        public override long TriggerBuildingId() { return BuildingTypes.Trainer; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            stateData.WorldSpriteName = CrawlerClientConstants.TrainerImage;
            PartyData party = _crawlerService.GetParty();

            foreach (PartyMember member in party.GetActiveParty())
            {

                ECrawlerStates nextState = ECrawlerStates.TrainingLevel;
                char nextKeyCode = (char)(member.PartySlot + '0');
                if (_combatService.IsDisabled(member))
                {
                    nextState = ECrawlerStates.None;
                    nextKeyCode = CharCodes.None;
                }

                stateData.Actions.Add(new CrawlerStateAction(member.PartySlot + " " + member.Name, nextKeyCode, nextState, extraData: member));
            }




            stateData.Actions.Add(new CrawlerStateAction("Back to the city", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
