using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Messages
{
    [MessagePackObject]
    public sealed class ServerMessageCounts : BaseMapApiMessage
    {
        [Key(0)] public long QueueCount { get; set; }
        [Key(1)] public long MinMessages { get; set; }
        [Key(2)] public long MaxMessages { get; set; }
        [Key(3)] public long TotalMessages { get; set; }
        [Key(4)] public long MessagesPerSecond { get; set; }
        [Key(5)] public long Seconds { get; set; }
        [Key(6)] public long TotalSpells { get; set; }
        [Key(7)] public long TotalUpdates { get; set; }
        [Key(8)] public MapObjectCounts MapCounts { get; set; }
        [Key(9)] public long PathfindingCount { get; set; }
    }
}
