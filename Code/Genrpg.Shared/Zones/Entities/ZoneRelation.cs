using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    /// <summary>
    /// Mark a zone as adjacent to this one.
    /// </summary>
    [MessagePackObject]
    public class ZoneRelation
    {
        [Key(0)] public long ZoneId { get; set; }
        [Key(1)] public float Offset { get; set; }
    }
}
