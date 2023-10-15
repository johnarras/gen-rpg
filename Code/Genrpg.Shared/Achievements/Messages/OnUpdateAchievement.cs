using Genrpg.Shared.MapMessages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Achievements.Messages
{
    [MessagePackObject]
    public sealed class OnUpdateAchievement : BaseMapApiMessage
    {
        [Key(0)] public int AchievementTypeId { get; set; }
        [Key(1)] public long Quantity { get; set; }
    }
}
