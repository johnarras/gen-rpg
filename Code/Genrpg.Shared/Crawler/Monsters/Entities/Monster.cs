using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    [MessagePackObject]
    public class Monster : CrawlerUnit
    {
        [Key(0)] public long MinDam { get; set; }
        [Key(1)] public long MaxDam { get; set; }
    }
}
