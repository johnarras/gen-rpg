using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneOverride
    {
        [Key(0)] public long ZoneTypeId { get; set; }
        [Key(1)] public float Scale { get; set; }
    }
}
