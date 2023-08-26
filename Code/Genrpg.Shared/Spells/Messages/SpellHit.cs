using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class SpellHit : BaseMapApiMessage
    {

        [Key(0)] public long Id { get; set; }

        [Key(1)] public Unit OrigTarget { get; set; }

        [Key(2)] public Unit Target { get; set; }

        [Key(3)] public int ProcDepth { get; set; }

        [Key(4)] public bool PrimaryTarget { get; set; }

        [Key(5)] public SendSpell SendSpell { get; set; }

        [Key(6)] public long BaseQuantity { get; set; }

        [Key(7)] public int VariancePct { get; set; }

        [Key(8)] public float CritMult { get; set; }

        [Key(9)] public float CritChance { get; set; }

        public SpellHit()
        {
        }
    }
}
