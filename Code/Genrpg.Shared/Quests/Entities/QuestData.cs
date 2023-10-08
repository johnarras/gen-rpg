using Genrpg.Shared.DataStores.PlayerData;
using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class QuestData : OwnerObjectList<QuestStatus>
    {
        [Key(0)] public override string Id { get; set; }

        private List<QuestStatus> _data { get; set; } = new List<QuestStatus>();

        public override List<QuestStatus> GetData()
        {
            return _data;
        }

        public override void SetData(List<QuestStatus> data)
        {
            _data = data;
        }

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
}
