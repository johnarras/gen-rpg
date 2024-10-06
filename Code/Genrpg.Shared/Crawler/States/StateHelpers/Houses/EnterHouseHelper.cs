using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Houses
{
    public class EnterHouseHelper : BaseStateHelper
    {
        private ICrawlerMapService _crawlerMapService;
        public override ECrawlerStates GetKey() { return ECrawlerStates.EnterHouse; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();


            PartyData party = _crawlerService.GetParty();

            int index = (party.MapX * 11 + party.MapZ * 31) % 5;

            stateData.WorldSpriteName = CrawlerClientConstants.HouseImage + (index + 1);


            if (_rand.NextDouble() < 0.3f)
            {
                stateData = new CrawlerStateData(ECrawlerStates.StartCombat, true)
                {
                    WorldSpriteName = CrawlerClientConstants.HouseImage + (index + 1),
                };
            }
            else
            {
                stateData.Actions.Add(new CrawlerStateAction("Exit House", CharCodes.Escape, ECrawlerStates.ExploreWorld));
                stateData.Actions.Add(new CrawlerStateAction("Exit House", CharCodes.Space, ECrawlerStates.ExploreWorld));
            }
            await Task.CompletedTask;
            return stateData;
        }
    }
}
