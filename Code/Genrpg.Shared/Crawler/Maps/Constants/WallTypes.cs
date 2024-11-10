using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Maps.Constants
{
    public class WallTypes
    {
        public const int None = 0;
        public const int Wall = 1;
        public const int Door = 2;
        public const int Secret = 3;
        public const int MaxDungeonWall = 3;
        public const int Barricade = 4;
        public const int Max = 5;

        public const int Building = 12;


        public static bool IsBlockingType(long wallType)
        {
            return wallType == Wall || wallType == Barricade || wallType == Secret;
        }
    }

}
