using MessagePack;
using Genrpg.Shared.Crawler.Maps.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.MapGen.Entities
{
    [MessagePackObject]
    public class NewCrawlerMap
    {
        [Key(0)] public CrawlerMap Map { get; set; }
        [Key(1)] public int EnterX { get; set; }
        [Key(2)] public int EnterZ { get; set; }
    }

}
