using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{

    public class FullWallTileImage
    {

        public int Index { get; set; }
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public long RotAngle { get; set; } = 0;

        public string ValText { get; set; }

        public WallTileImage RefImage { get; set; }
    }

}
