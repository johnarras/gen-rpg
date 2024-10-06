using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    internal class CrawlerCharacterScreenData
    {
        public CrawlerCharacterScreenData()
        {
        }

        public PartyMember Unit { get; set; }
        public ECrawlerStates PrevState { get; set; }
    }
}