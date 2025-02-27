using Genrpg.Shared.Utils;
using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneFenceType : IWeightedItem
    {
        [Key(0)] public long FenceTypeId { get; set; }
        [Key(1)] public double Weight { get; set; } = 1;
        [Key(2)] public string Name { get; set; }

    }
}
