using MessagePack;
namespace Genrpg.Shared.Zones.WorldData
{
    /// <summary>
    /// Used to override data about plant types in the zone type and zone
    /// </summary>
    [MessagePackObject]
    public class ZonePlantType
    {
        [Key(0)] public long PlantTypeId { get; set; }
        [Key(1)] public float Density { get; set; }

        public ZonePlantType()
        {
            Density = 1.0f;
        }
    }
}
