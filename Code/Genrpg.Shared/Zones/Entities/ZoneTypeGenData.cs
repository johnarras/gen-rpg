using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneTypeGenData
    {
        [Key(0)] public ZoneType zoneType { get; set; }
        [Key(1)] public float chance { get; set; }
    }
}
