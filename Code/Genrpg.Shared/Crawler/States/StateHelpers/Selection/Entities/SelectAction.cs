using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities
{
    public class SelectAction
    {
        public PartyMember Member { get; set; }
        public UnitAction Action { get; set; }
        public ECrawlerStates ReturnState { get; set; }
        public ECrawlerStates NextState { get; set; }
    }
}
