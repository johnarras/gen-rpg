using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;

namespace Assets.Scripts.Crawler.StateHelpers.Selection.Entities
{
    public class SelectAction
    {
        public PartyMember Member { get; set; }
        public UnitAction Action { get; set; }
        public ECrawlerStates ReturnState { get; set; }
        public ECrawlerStates NextState { get; set; }
    }
}
