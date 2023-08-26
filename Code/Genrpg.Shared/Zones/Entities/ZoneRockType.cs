using MessagePack;
using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.Zones.Entities
{
    /// <summary>
    /// Used to override data about plant types in the zone type and zone
    /// </summary>
    [MessagePackObject]
    public class ZoneRockType
    {
        [Key(0)] public long RockTypeId { get; set; }
        [Key(1)] public float ChanceScale { get; set; }


        [Key(2)] public MyColorF BaseColor { get; set; }

        public ZoneRockType()
        {
            ChanceScale = 1.0f;


            BaseColor = new MyColorF();
        }
    }
}
