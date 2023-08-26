using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnStartCast : BaseMapApiMessage
    {
        [Key(0)] public string CasterId { get; set; }
        [Key(1)] public float CastSeconds { get; set; }
        [Key(2)] public string CastingName { get; set; }
        [Key(3)] public string AnimName { get; set; }
    }
}
