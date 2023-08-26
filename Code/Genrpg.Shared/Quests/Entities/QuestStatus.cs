using MessagePack;

using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;

namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class QuestStatus : IStatusItem
    {
        [Key(0)] public int Id { get; set; }

        [Key(1)] public QuestType Quest { get; set; }
        [Key(2)] public List<QuestTaskStatus> Statuses { get; set; }



        public bool IsComplete()
        {
            if (Quest == null || Quest.Tasks == null || Statuses == null)
            {
                return false;
            }


            foreach (QuestTask task in Quest.Tasks)
            {
                QuestTaskStatus status = Statuses.FirstOrDefault(x => x.Index == task.Index);
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

        public QuestStatus()
        {
            Statuses = new List<QuestTaskStatus>();
        }
    }

    [MessagePackObject]
    public class QuestData : ObjectList<QuestStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<QuestStatus> Data { get; set; } = new List<QuestStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

        public QuestStatus GetStatus(QuestType qtype)
        {
            if (qtype == null)
            {
                return null;
            }

            return Data.FirstOrDefault(x => x.Quest.IsSameQuest(qtype));
        }

        public void AddStatus(QuestStatus status)
        {
            if (status == null || status.Quest == null)
            {
                return;
            }

            if (Data == null)
            {
                Data = new List<QuestStatus>();
            }

            QuestStatus currStatus = GetStatus(status.Quest);

            if (currStatus != null)
            {
                return;
            }

            Data.Add(status);

        }

        public void RemoveStatus(QuestStatus status)
        {
            if (status == null || status.Quest == null)
            {
                return;
            }

            if (Data == null)
            {
                return;
            }

            QuestStatus currStatus = GetStatus(status.Quest);

            if (currStatus != null)
            {
                Data.Remove(currStatus);
            }
        }

    }

}
