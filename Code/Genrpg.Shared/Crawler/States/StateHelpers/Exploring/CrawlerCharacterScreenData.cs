using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    [MessagePackObject]
    public class CrawlerCharacterScreenData
    {
        public CrawlerCharacterScreenData()
        {
        }

        [Key(0)] public PartyMember Unit { get; set; }
        [Key(1)] public ECrawlerStates PrevState { get; set; }
    }
}
