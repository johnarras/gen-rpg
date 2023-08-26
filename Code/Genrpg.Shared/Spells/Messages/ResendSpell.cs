using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class ResendSpell : BaseMapApiMessage
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public long ShotsLeft { get; set; }
        [Key(2)] public SendSpell SpellMessage { get; set; }
    }
}
