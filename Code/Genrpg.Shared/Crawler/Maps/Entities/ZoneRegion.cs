using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class ZoneRegion
    {
        [Key(0)] public long ZoneTypeId { get; set; }
        [Key(1)] public string Name { get; set; }
        [Key(2)] public int CenterX { get; set; }
        [Key(3)] public int CenterY { get; set; }
        [Key(4)] public float SpreadX { get; set; }
        [Key(5)] public float SpreadY { get; set; }
        [Key(6)] public float DirX { get; set; }
        [Key(7)] public float DirY { get; set; }
        [Key(8)] public int Level { get; set; }
    }
}
