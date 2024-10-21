using MessagePack;
using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{
    [MessagePackObject]
    public class WallTileImage
    {
        [Key(0)] public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        [Key(1)] public string Filename { get; set; } = "OOOO";

    }

}
