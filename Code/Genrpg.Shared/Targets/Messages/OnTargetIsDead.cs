using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Targets.Messages
{
    [MessagePackObject]
    public sealed class OnTargetIsDead : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
    }
}
