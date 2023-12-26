using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Pathfinding.Entities
{
    [MessagePackObject]
    public class Waypoint
    {
        [Key(0)] public int X { get; set; }
        [Key(1)] public int Z { get; set; }
    }
}
