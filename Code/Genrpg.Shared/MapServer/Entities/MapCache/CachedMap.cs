using MessagePack;

using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapServer.Entities.MapCache
{
    [MessagePackObject]
    public class CachedMap
    {
        [Key(0)] public Map FullMap { get; set; }
        [Key(1)] public Map ClientMap { get; set; }
        [Key(2)] public bool Generating { get; set; }
        [Key(3)] public List<CachedMapInstance> Instances { get; set; } = new List<CachedMapInstance>();

    }
}
