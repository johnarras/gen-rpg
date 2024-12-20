using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Loot.Messages
{
    [MessagePackObject]
    public sealed class SendRewards : BaseMapApiMessage
    {
        [Key(0)] public bool ShowPopup { get; set; }
        [Key(1)] public List<RewardList> Rewards { get; set; }
    }
}
