﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class ZoneRegion
    {
        public long ZoneTypeId { get; set; }
        public string Name { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public float SpreadX { get; set; }
        public float SpreadY { get; set; }
        public float DirX { get; set; }
        public float DirY { get; set; }
        public int Level { get; set; }
    }
}
