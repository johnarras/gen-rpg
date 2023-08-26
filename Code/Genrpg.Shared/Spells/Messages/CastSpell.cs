using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class CastSpell : BaseMapApiMessage, IPlayerCommand
    {
        [Key(0)] public long SpellId { get; set; }
        [Key(1)] public string TargetId { get; set; }
    }
}
