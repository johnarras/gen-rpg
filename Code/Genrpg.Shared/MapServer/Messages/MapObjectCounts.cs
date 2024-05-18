using Genrpg.Shared.MapMessages;
using MessagePack;

namespace Genrpg.Shared.MapServer.Messages
{
    [MessagePackObject]
    public sealed class MapObjectCounts : BaseMapApiMessage
    {
        [Key(0)] public long CurrentObjectCount { get; set; }
        [Key(1)] public long CurrentUnitCount { get; set; }
        [Key(2)] public long UnitsAdded { get; set; }
        [Key(3)] public long UnitsRemoved { get; set; }
        [Key(4)] public long TotalQueries { get; set; }
        [Key(5)] public long TotalGridReads { get; set; }
        [Key(6)] public long TotalGridLocks { get; set; }
        [Key(7)] public long UnitTotal { get; set; }
        [Key(8)] public long ObjectTotal { get; set; }
        [Key(9)] public long IdLookupObjectAccount { get; set; }
        [Key(10)] public long GridObjectCount { get; set; }
        [Key(11)] public long ZoneObjectCount { get; set; }

    }
}
