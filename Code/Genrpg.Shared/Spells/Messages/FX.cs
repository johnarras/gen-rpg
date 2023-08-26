using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class FX : BaseMapApiMessage
    {
        [Key(0)] public string From { get; set; }
        [Key(1)] public string To { get; set; }
        [Key(2)] public string Art { get; set; }
        [Key(3)] public float Dur { get; set; }
        [Key(4)] public float Speed { get; set; }
    }
}
