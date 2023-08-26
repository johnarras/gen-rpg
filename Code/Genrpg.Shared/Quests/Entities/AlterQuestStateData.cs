using MessagePack;
namespace Genrpg.Shared.Quests.Entities
{

    public class AlterQuestType
    {
        public const int Accept = 1;
        public const int Abandon = 2;
        public const int Complete = 3;
    }


    [MessagePackObject]
    public class AlterQuestStateData
    {
        [Key(0)] public long AlterTypeId { get; set; }
        [Key(1)] public long QuestTypeId { get; set; }
        [Key(2)] public string MapId { get; set; }

        [Key(3)] public int MapVersion { get; set; }
        [Key(4)] public long NPCTypeId { get; set; }

    }
}
