using MessagePack;
using Genrpg.Shared.Spells.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;

namespace Genrpg.Shared.Spells.Messages
{
    [MessagePackObject]
    public sealed class OnAddEffect : BaseMapApiMessage, IEffect
    {
        [Key(0)] public long Id { get; set; }
        [Key(1)] public string TargetId { get; set; } = null!;
        [Key(2)] public string Icon { get; set; } = null!;
        [Key(3)] public float Duration { get; set; }
        [Key(4)] public float DurationLeft { get; set; }
        [Key(5)] public long EntityTypeId { get; set; }
        [Key(6)] public long Quantity { get; set; }
        [Key(7)] public long EntityId { get; set; }
    }
}
