using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class Stat
    {
        [Key(0)] public short Id { get; set; }
        [Key(1)] public int Val { get; set; }
    }
}
