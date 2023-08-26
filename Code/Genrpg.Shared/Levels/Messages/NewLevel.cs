using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Levels.Messages
{
    [MessagePackObject]
    public sealed class NewLevel : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public long Level { get; set; }
    }
}
