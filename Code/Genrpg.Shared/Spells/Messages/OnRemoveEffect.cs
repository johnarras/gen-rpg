using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnRemoveEffect : BaseMapApiMessage
    {
        [Key(0)] public string TargetId { get; set; }
        [Key(1)] public long Id { get; set; }
    }
}
