using MessagePack;
namespace Genrpg.Shared.Zones.Entities
{
    [MessagePackObject]
    public class ZoneTypeOverride
    {
        /// <summary>
        /// Zone type to transition to
        /// </summary>
        [Key(0)] public long ZoneTypeId { get; set; }
        /// <summary>
        /// The reason for the override, cold, hot, wet, dry, radiation...
        /// </summary>
        [Key(1)] public int Reason { get; set; }
    }
}
