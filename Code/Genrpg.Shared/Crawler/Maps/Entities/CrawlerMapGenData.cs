using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class CrawlerMapGenData
    {
        public CrawlerWorld World;
        public long MapType;
        public int Level { get; set; } = 0;
        public long ZoneTypeId { get; set; }
        public bool Looping { get; set; }
        public long FromMapId { get; set; }
        public int FromMapX { get; set; }
        public int FromMapZ { get; set; }
        public int CurrFloor { get; set; } = 1;
        public int MaxFloor { get; set; } = 1;
        public string Name { get; set; }
        public bool SimpleDungeon { get; set; }
        public CrawlerMap PrevMap { get; set; }
    }

}
