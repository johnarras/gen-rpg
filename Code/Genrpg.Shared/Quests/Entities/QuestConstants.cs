using MessagePack;
namespace Genrpg.Shared.Quests.Entities
{
    [MessagePackObject]
    public class QuestConstants
    {

        public const int QuestLevelBelowZoneLevel = 4;
        public const int QuestAlmostVisibleLevels = 3;

        public const float QuestKillMoneyMultiplier = 10;
    }

    [MessagePackObject]
    public class QuestState
    {
        public const int NotAvailable = 0;
        public const int AlmostAvailable = 1;
        public const int Available = 2;
        public const int Active = 3;
        public const int Complete = 4;
        public const int AlreadyCompleted = 5;
    }
}
