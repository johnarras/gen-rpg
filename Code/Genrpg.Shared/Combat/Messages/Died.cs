using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.MapMessages;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Combat.Messages
{
    [MessagePackObject]
    public sealed class Died : BaseMapApiMessage
    {
        [Key(0)] public string UnitId { get; set; }
        [Key(1)] public List<Reward> Loot { get; set; }
        [Key(2)] public List<Reward> SkillLoot { get; set; }
        [Key(3)] public AttackerInfo FirstAttacker { get; set; }
    }
}
