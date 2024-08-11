using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Messages
{
    [MessagePackObject]
    public sealed class OnAddQuantityReward : BaseMapApiMessage
    {
        [Key(0)] public string CharId { get; set; }
        [Key(1)] public long EntityTypeId { get; set; }
        [Key(2)] public long EntityId { get; set; }
        [Key(3)] public long Quantity { get; set; }
    }
}
