using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class MapEncounters
    {
        public const int OtherFeature = 1 << 0;
        public const int Treasure = 1 << 1;
        public const int Monsters = 1 << 2;
        public const int Trap = 1 << 3;
    }
}
