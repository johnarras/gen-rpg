using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;
using System;
using System.Security;

namespace Genrpg.Shared.Crawler.Training.Settings
{
    [MessagePackObject]
    public class CrawlerTrainingSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long LinearCostPerLevel { get; set; } = 100;
        [Key(2)] public long QuadraticCostPerLevel { get; set; } = 10;

        [Key(3)] public double StartKillsNeeded { get; set; } = 20;
        [Key(4)] public double ExtraKillsNeeded { get; set; } = 2;

        [Key(5)] public double StartMonsterExp { get; set; } = 100;
        [Key(6)] public double ExtraMonsterExp { get; set; } = 25;

        [Key(7)] public long MaxScalingExpLevel { get; set; } = 25;

        [Key(8)] public double LowerStatIncreaseChance { get; set; } = 0.75;
        [Key(9)] public double MaxStatIncreaseChance { get; set; } = 0.50;

        public long GetExpToLevel(long level)
        {
            if (level < 1)
            {
                level = 1;
            }

            if (level > MaxScalingExpLevel)
            {
                level = MaxScalingExpLevel;
            }

            return (long)((StartKillsNeeded + ExtraKillsNeeded * (level - 1)) *
                (StartMonsterExp + ExtraMonsterExp * (level - 1)));

        }

        public long GetNextLevelTrainingCost(long currentLevel)
        {
            currentLevel--;
            return LinearCostPerLevel * (currentLevel + 1) + QuadraticCostPerLevel * currentLevel * currentLevel;
        }

        public long GetMonsterExp(long currentLevel)
        {
            if (currentLevel > MaxScalingExpLevel)
            {
                currentLevel = MaxScalingExpLevel;
            }
            return (long)(StartMonsterExp + ExtraMonsterExp * (currentLevel - 1));
        }
    }


    [MessagePackObject]
    public class CrawlerTrainingSettingsLoader : NoChildSettingsLoader<CrawlerTrainingSettings> { }



    [MessagePackObject]
    public class CrawlerTrainingSettingsMapper : NoChildSettingsMapper<CrawlerTrainingSettings> { }
}
