using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapObjects.Entities
{
    [MessagePackObject]
    public class MapObjectGridItem
    {
        [Key(0)] public MapObject Obj { get; set; }
        [Key(1)] public int GX { get; set; }
        [Key(2)] public int GZ { get; set; }
        [Key(3)] public int OldGX { get; set; }
        [Key(4)] public int OldGZ { get; set; }
    }
}
