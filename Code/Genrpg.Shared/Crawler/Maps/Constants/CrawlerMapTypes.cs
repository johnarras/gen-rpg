using Genrpg.Shared.Characters.PlayerData;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Constants
{

    public class CrawlerMapTypes
    {
        public const long None = 0;
        public const long Dungeon = 1;
        public const long City = 2;
        public const long Outdoors = 3;
        public const long Cave = 4;
        public const long Tower = 5;





        public const long RandomDungeon = 9999;
    }
}
