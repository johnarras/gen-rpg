using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnStopCast : BaseMapApiMessage
    {
        [Key(0)] public string CasterId { get; set; }
    }
}
