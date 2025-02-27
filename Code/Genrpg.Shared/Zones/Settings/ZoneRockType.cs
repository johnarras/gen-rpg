using MessagePack;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Zones.Settings
{
    /// <summary>
    /// Used to override data about plant types in the zone type and zone
    /// </summary>
    [MessagePackObject]
    public class ZoneRockType : IWeightedItem
    {
        [Key(0)] public long RockTypeId { get; set; }
        [Key(1)] public double Weight { get; set; } = 1.0f;
        [Key(2)] public string Name { get; set; }


        [Key(3)] public MyColorF BaseColor { get; set; }

        public ZoneRockType()
        {
            BaseColor = new MyColorF();
        }
    }
}
