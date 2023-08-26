using MessagePack;
using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;

namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class AlterQuestStateResult
    {
        [Key(0)] public long AlterTypeId { get; set; }
        [Key(1)] public QuestStatus Status { get; set; }

        [Key(2)] public List<SpawnResult> Rewards { get; set; }

        [Key(3)] public string Message { get; set; }

        [Key(4)] public bool Success { get; set; }
    }
}
