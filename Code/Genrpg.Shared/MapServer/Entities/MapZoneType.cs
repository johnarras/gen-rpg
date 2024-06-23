using MessagePack;
namespace Genrpg.Shared.MapServer.Entities
{
    [MessagePackObject]
    public class MapZoneType
    {
        [Key(0)] public long ZoneTypeId { get; set; }
        [Key(1)] public float GenChance { get; set; }
        [Key(2)] public string Name { get; set; }
    }
}
