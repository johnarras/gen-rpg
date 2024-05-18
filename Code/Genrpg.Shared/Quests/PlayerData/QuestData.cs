using Genrpg.Shared.Charms.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

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
    [MessagePackObject]
    public class QuestData : OwnerObjectList<QuestStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public QuestStatus GetStatus(QuestType qtype)
        {
            if (qtype == null)
            {
                return null;
            }

            return _data.FirstOrDefault(x => x.Quest.IsSameQuest(qtype));
        }

        public void AddStatus(QuestStatus status)
        {
            if (status == null || status.Quest == null)
            {
                return;
            }

            QuestStatus currStatus = GetStatus(status.Quest);

            if (currStatus != null)
            {
                return;
            }

            _data.Add(status);

        }

        public void RemoveStatus(QuestStatus status)
        {
            if (status == null || status.Quest == null)
            {
                return;
            }

            if (_data == null)
            {
                return;
            }

            QuestStatus currStatus = GetStatus(status.Quest);

            if (currStatus != null)
            {
                _data.Remove(currStatus);
            }
        }

    }

    [MessagePackObject]
    public class QuestApi : OwnerApiList<QuestData, QuestStatus> { }
    
    [MessagePackObject]
    public class QuestDataLoader : OwnerDataLoader<QuestData, QuestStatus> { }


    [MessagePackObject]
    public class QuestDataMapper : OwnerDataMapper<QuestData, QuestStatus, QuestApi> { }
}
