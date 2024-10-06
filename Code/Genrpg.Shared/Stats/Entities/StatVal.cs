using MessagePack;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatVal
    {
        [Key(0)] public short StatTypeId { get; set; }
        [Key(1)] public int Val { get; set; }
    }
}
