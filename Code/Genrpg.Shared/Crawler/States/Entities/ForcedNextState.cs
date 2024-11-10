using MessagePack;

using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.States.Constants;

namespace Genrpg.Shared.Crawler.States.Entities
{
    [MessagePackObject]
    public class ForcedNextState
    {
        [Key(0)] public ECrawlerStates NextState { get; set; }
        [Key(1)] public MapCellDetail Detail { get; set; }
    }
}
