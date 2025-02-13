﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class CrawlerMapConstants
    {
        public const int BlockCount = 128;

        public const int BlockSize = 8;

        public const int MapSize = BlockCount * BlockSize + 1;

        public const int CityDirBitShiftSize = 5;

        public const int DirToAngleMult = 90;

        public const double TreeChanceScale = 0.25f;
    }
}
