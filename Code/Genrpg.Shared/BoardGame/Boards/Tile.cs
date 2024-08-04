using MessagePack;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Boards
{
    [MessagePackObject]
    public class Tile
    {
        [Key(0)] public long TileTypeId { get; set; }
    }
}
