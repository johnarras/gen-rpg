using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class TargetCastState
    {
        [Key(0)] public Unit Target { get; set; }
        [Key(1)] public TryCastState State { get; set; }
    }
}
