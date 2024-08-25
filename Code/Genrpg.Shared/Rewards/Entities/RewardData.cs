using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Entities
{
    [MessagePackObject]
    public class RewardData
    {
        [Key(0)] public List<RewardList> Rewards { get; set; } = new List<RewardList>();
    }
}
