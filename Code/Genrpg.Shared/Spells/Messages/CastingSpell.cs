using MessagePack;
using Genrpg.Shared.Spells.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class CastingSpell : BaseMapApiMessage, ICastTimeMessage
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public Spell Spell { get; set; }
        [Key(2)] public float CastingTime { get; set; }
        [Key(3)] public DateTime EndCastingTime { get; set; }
    }
}
