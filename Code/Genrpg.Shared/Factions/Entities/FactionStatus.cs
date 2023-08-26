using MessagePack;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Factions.Entities
{
    [MessagePackObject]
    public class FactionStatus : IStatusItem, IId
    {
        
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public long RepLevelId { get; set; }
        [Key(2)] public long Reputation { get; set; }
    }
}
