using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.Shared.Factions.Messages
{
    [MessagePackObject]
    public class RepResult
    {
        [Key(0)] public long FactionTypeId { get; set; }
        [Key(1)] public long OldRepLevelId { get; set; }
        [Key(2)] public long NewRepLevelId { get; set; }
        [Key(3)] public long OldRep { get; set; }
        [Key(4)] public long NewRep { get; set; }
        [Key(5)] public long RepChange { get; set; }
        [Key(6)] public List<Reward> Rewards { get; set; }
    }
}
