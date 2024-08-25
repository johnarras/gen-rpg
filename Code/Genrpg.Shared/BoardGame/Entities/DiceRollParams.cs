using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Entities
{
    [MessagePackObject]
    public class DiceRollParams
    {
        [Key(0)] public long ForceRoll { get; set; }
        [Key(1)] public bool IsFree { get; set; }
    }
}
