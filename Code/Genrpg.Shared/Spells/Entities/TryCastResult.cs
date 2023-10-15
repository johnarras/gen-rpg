using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class TryCastResult
    {
        public TryCastState State;
        [Key(0)] public Unit Target { get; set; }
        [Key(1)] public Spell Spell { get; set; }
        [Key(2)] public ElementType ElementType { get; set; }
        [Key(3)] public string StateText { get; set; }
    }
}
