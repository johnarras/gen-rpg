using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateEffect : BaseMapApiMessage
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public float Duration { get; set; }
        [Key(2)] public float DurationLeft { get; set; }
    }
}
