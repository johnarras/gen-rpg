﻿using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class NaturalCrawlerMapTypeHelper : BaseDungeonCrawlerMapTypeHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Natural; }


        protected override bool IsIndoors() { return false; }
    }
}
