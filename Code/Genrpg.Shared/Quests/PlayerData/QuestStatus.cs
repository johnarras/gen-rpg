using MessagePack;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Quests.WorldData;

namespace Genrpg.Shared.Quests.PlayerData
{
    [MessagePackObject]
    public class QuestStatus : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }

        [Key(2)] public QuestType Quest { get; set; }
        [Key(3)] public List<QuestTaskStatus> Tasks { get; set; } = new List<QuestTaskStatus>();

        public bool IsComplete()
        {
            if (Quest == null || Quest.Tasks == null || Tasks == null)
            {
                return false;
            }

            foreach (QuestTask task in Quest.Tasks)
            {
                QuestTaskStatus status = Tasks.FirstOrDefault(x => x.Index == task.Index);
                if (status == null)
                {
                    return false;
                }

                if (status.CurrQuantity < task.Quantity)
                {
                    return false;
                }
            }
            return true;

        }
    }
}
