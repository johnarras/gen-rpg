using MessagePack;
using Genrpg.Shared.MapMessages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pathfinding.Messages
{
    [MessagePackObject]
    public sealed class OnMoveToLocation : BaseMapApiMessage
    {
        [Key(0)] public string ObjId { get; set; }
        [Key(3)] public float Speed { get; set; }
        [Key(1)] public short FinalX { get; set; }
        [Key(2)] public short FinalZ { get; set; }   
        [Key(4)] public short ObjX { get; set; }
        [Key(5)] public short ObjZ { get; set; }
    }
}
