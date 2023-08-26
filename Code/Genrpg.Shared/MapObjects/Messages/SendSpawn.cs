using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class SendSpawn : BaseMapApiMessage
    {
        [Key(0)] public string ToObjId { get; set; }
    }
}
