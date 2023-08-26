using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Messages
{
    [MessagePackObject]
    public sealed class GetSpawnedObject : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public string ObjId { get; set; }
    }
}
