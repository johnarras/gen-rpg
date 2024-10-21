using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class EnterCrawlerMapData
    {
        [Key(0)] public long MapId { get; set; }
        [Key(1)] public int MapX { get; set; }
        [Key(2)] public int MapZ { get; set; }
        [Key(3)] public int MapRot { get; set; }

        [Key(4)] public CrawlerWorld World { get; set; }
        [Key(5)] public CrawlerMap Map { get; set; }
    }
}
