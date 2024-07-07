using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Constants
{
    public class MapWallBits
    {
        public const int EWallStart = 0; // 0
        public const int NWallStart = EWallStart + WallBitSize; // 90

        public const int WallBitSize = 2;

        public const int IsRoomBitOffset = NWallStart + WallBitSize + 1;
    }

}
