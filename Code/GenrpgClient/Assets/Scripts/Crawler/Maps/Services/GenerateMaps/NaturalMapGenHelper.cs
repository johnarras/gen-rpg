﻿using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class NaturalMapGenHelper : BaseDungeonMapGenHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Natural; }
    }
}
