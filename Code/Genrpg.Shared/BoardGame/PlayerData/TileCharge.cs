using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    [MessagePackObject]
    public class TileCharge
    {
        [Key(0)] public int TileIndex { get; set; }
        [Key(1)] public int CurrCharge { get; set; }
        [Key(2)] public int MaxCharge { get; set; }
    }
}
