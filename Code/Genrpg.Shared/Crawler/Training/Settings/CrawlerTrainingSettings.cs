using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System.Security;

namespace Genrpg.Shared.Crawler.Training.Settings
{
    [MessagePackObject]
    public class CrawlerTrainingSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long LinearCostPerLevel { get; set; } = 100;
        [Key(2)] public long QuadraticCostPerLevel { get; set; } = 10;

        [Key(3)] public long StartKillsNeeded { get; set; } = 20;
        [Key(4)] public long ExtraKillsNeeded { get; set; } = 2;

        [Key(5)] public long StartMonsterExp { get; set; } = 100;
        [Key(6)] public long ExtraMonsterExp { get; set; } = 25;

        public long GetExpToLevel(long level)
        {
            if (level < 1)
            {
                level = 1;
            }

            return (StartKillsNeeded + ExtraKillsNeeded * (level - 1)) *
                (StartMonsterExp + ExtraMonsterExp * (level - 1));

        }

        public long GetNextLevelTrainingCost(long currentLevel)
        {
            currentLevel--;
            return LinearCostPerLevel * (currentLevel + 1) + QuadraticCostPerLevel * currentLevel * currentLevel;
        }

        public long GetMonsterExp(long currentLevel)
        {
            return StartMonsterExp + ExtraMonsterExp * (currentLevel - 1);
        }
    }


    [MessagePackObject]
    public class CrawlerTrainingSettingsLoader : NoChildSettingsLoader<CrawlerTrainingSettings> { }
}
