using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Targets.Messages
{
    [MessagePackObject]
    public sealed class OnSetTarget : BaseMapApiMessage
    {
        [Key(0)] public string CasterId { get; set; }
        [Key(1)] public string TargetId { get; set; }
    }
}
