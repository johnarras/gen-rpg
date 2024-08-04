using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Boards
{
    [MessagePackObject]
    public class Board : IIndexedGameItem
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public List<Tile> Tiles { get; set; } = new List<Tile>();
    }
}
