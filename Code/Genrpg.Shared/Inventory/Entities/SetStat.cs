using MessagePack;
using Genrpg.Shared.Stats.Entities;

namespace Genrpg.Shared.Inventory.Entities
{
    [MessagePackObject]
    public class SetStat : IStatPct
    {
        [Key(0)] public int ItemCount { get; set; }
        [Key(1)] public long StatTypeId { get; set; }
        [Key(2)] public int Percent { get; set; }
    }
}
