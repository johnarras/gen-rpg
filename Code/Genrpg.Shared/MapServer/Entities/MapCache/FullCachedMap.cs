using MessagePack;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Entities.MapCache
{
    [MessagePackObject]
    public class FullCachedMap
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public CachedMapInstance MapInstance { get; set; }
    }
}
