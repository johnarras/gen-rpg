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
        [Key(2)] public int StartPoints { get; set; } = 10;


        /// <summary>
        /// Costs are StartPoints for first tier of first building and
        /// increase by 1 across all tier 1, then up to all tier 5.
        /// </summary>
        /// <param name="maxBuildingQuantity"></param>
        /// <returns></returns>
        public long GetTotalUpgradePoints(int maxBuildingQuantity)
        {
            int rowDeltaPoints = maxBuildingQuantity * (maxBuildingQuantity + 1) / 2;
            int allDeltaPoints = rowDeltaPoints * MaxTiers;

            // First row base points
            int firstRowBasePoints = StartPoints * maxBuildingQuantity;

            // Now it's Tier(Tier+1)/2*firstRowBasePoints for all tiers

            int allRowBasePoints = (MaxTiers * (MaxTiers + 1) / 2) * firstRowBasePoints;

            return allDeltaPoints + allRowBasePoints;
        }

        public long GetUpgradePoints(long maxBuildingQuantity, int buildingIndex, int nextTier)
        {
            return StartPoints + (maxBuildingQuantity * (nextTier - 1)) + buildingIndex;
        }
    }


    [MessagePackObject]
    public class UpgradeBoardSettingsLoader : NoChildSettingsLoader<UpgradeBoardSettings> { }



    [MessagePackObject]
    public class UpgradeBoardSettingsMapper : NoChildSettingsMapper<UpgradeBoardSettings> { }
}
