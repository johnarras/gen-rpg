using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class ReturnToSafetyHelper : BaseStateHelper
    {
        public override ECrawlerStates GetKey() {  return ECrawlerStates.ReturnToSafety; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData partyData = _crawlerService.GetParty();

            CrawlerMap map = _worldService.GetMap(partyData.MapId);

            CrawlerWorld world = await _worldService.GetWorld(partyData.WorldId);



            stateData.Actions.Add(new CrawlerStateAction("Do you wish to return to the starting city?"));
            stateData.Actions.Add(new CrawlerStateAction("You will lose half of your gold."));


            EnterCrawlerMapData safetyData = new EnterCrawlerMapData()
            {
                ReturnToSafety = true,
            };

            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld,
                () =>
                {
                    partyData.Gold /= 2;
                }, safetyData));

               

            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

            return stateData;
        }
    }
}
