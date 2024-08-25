using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class UpgradeBoardSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int MaxTiers { get; set; } = 5;
        [Key(2)] public int StartUpgradeWeight { get; set; } = 10;

        /// <summary>
        /// Starting upgrade days cost
        /// </summary>
        [Key(3)] public double StartUpgradeDays { get; set; } = 0.3;

        /// <summary>
        /// How many extra upgrade days are added per level
        /// </summary>
        [Key(4)] public double UpgradeDaysPerLevel { get; set; } = 0.1;

        /// <summary>
        /// Max cost in days for a full update
        /// </summary>
        [Key(5)] public double MaxUpgradeDays { get; set; } = 3.0;

        /// <summary>
        /// How many hours of energy collection is considered to be a full day
        /// </summary>
        [Key(6)] public long FullDayEnergyCollectionHours { get; set; } = 12;


        public double GetUpgradeDays(long level)
        {
            return Math.Min(MaxUpgradeDays, StartUpgradeDays + level * UpgradeDaysPerLevel);
        }


        /// <summary>
        /// Costs are StartPoints for first tier of first building and
        /// increase by 1 across all tier 1, then up to all tier 5.
        /// </summary>
        /// <param name="maxUpgradeObjectQuantity"></param>
        /// <returns></returns>
        public long GetTotalUpgradeWeight(long maxUpgradeObjectQuantity)
        {
            long rowDeltaPoints = maxUpgradeObjectQuantity * (maxUpgradeObjectQuantity + 1) / 2;
            long allDeltaPoints = rowDeltaPoints * MaxTiers;

            // First row base points
            long firstRowBasePoints = StartUpgradeWeight * maxUpgradeObjectQuantity;

            // Now it's Tier(Tier+1)/2*firstRowBasePoints for all tiers

            long allRowBasePoints = (MaxTiers * (MaxTiers + 1) / 2) * firstRowBasePoints;

            return allDeltaPoints + allRowBasePoints;
        }

        public long GetCurrentUpgradeWeight(long maxUpgradeQuantity, long upgradeIndex, long nextTier)
        {
            return StartUpgradeWeight + (maxUpgradeQuantity * (nextTier - 1)) + upgradeIndex;
        }
    }


    [MessagePackObject]
    public class UpgradeBoardSettingsLoader : NoChildSettingsLoader<UpgradeBoardSettings> { }



    [MessagePackObject]
    public class UpgradeBoardSettingsMapper : NoChildSettingsMapper<UpgradeBoardSettings> { }
}
