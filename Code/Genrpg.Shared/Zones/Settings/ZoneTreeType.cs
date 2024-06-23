using MessagePack;
namespace Genrpg.Shared.Zones.Settings
{
    /// <summary>
    /// Used to override data about trees in the zone and zone type
    /// </summary>
    [MessagePackObject]
    public class ZoneTreeType
    {
        [Key(0)] public long TreeTypeId { get; set; }
        [Key(1)] public float PopulationScale { get; set; }
        [Key(2)] public string Name { get; set; }

        public ZoneTreeType()
        {
            PopulationScale = 1.0f;
        }
    }
}
