using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public struct Stat
    {
        [Key(0)] public int Val { get; set; }
    }
}
