using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Networking.Messages
{
    [MessagePackObject]
    public sealed class ConnMessageCounts : BaseMapApiMessage
    {
        [Key(0)] public long MessagesSent { get; set; }
        [Key(1)] public long MessagesReceived { get; set; }
        [Key(2)] public long BytesSent { get; set; }
        [Key(3)] public long BytesReceived { get; set; }
        [Key(4)] public long Seconds { get; set; }
    }
}
