using MessagePack;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Selection.Entities
{
    [MessagePackObject]
    public class SelectAction
    {
        [Key(0)] public PartyMember Member { get; set; }
        [Key(1)] public UnitAction Action { get; set; }
        [Key(2)] public ECrawlerStates ReturnState { get; set; }
        [Key(3)] public ECrawlerStates NextState { get; set; }
    }
}
