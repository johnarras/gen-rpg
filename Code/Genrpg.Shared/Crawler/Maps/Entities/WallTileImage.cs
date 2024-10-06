using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    public class WallTileImage
    {
        public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        public string Filename { get; set; } = "OOOO";

    }

}
