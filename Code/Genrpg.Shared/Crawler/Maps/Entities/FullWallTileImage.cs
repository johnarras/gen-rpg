using MessagePack;
using Genrpg.Shared.Crawler.Maps.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Maps.Entities
{

    [MessagePackObject]
    public class FullWallTileImage
    {

        [Key(0)] public int Index { get; set; }
        [Key(1)] public int[] WallIds { get; set; } = new int[TileImageConstants.WallCount];
        [Key(2)] public long RotAngle { get; set; } = 0;

        [Key(3)] public string ValText { get; set; }

        [Key(4)] public WallTileImage RefImage { get; set; }
    }

}
