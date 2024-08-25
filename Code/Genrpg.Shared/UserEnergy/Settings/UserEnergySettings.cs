using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.UserEnergy.Settings
{
    [MessagePackObject]
    public class UserEnergySettings : NoChildSettings
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double HourlyRegenPercent { get; set; } = 0.25;

        [Key(2)] public int StartStorage { get; set; } = 40;

        [Key(3)] public int LevelsPerIncrement { get; set; } = 5;

        [Key(4)] public int IncrementQuantity { get; set; } = 5;

        [Key(5)] public int StorageCap { get; set; } = 80;

        public int GetMaxStorage(long level)
        {
            return (int)Math.Min(StorageCap, StartStorage + level / LevelsPerIncrement * IncrementQuantity);
        }

        public double EnergyPerHour(long level)
        {
            return GetMaxStorage(level) * HourlyRegenPercent;
        }

    }
    [MessagePackObject]
    public class UserEnergySettingsLoader : NoChildSettingsLoader<UserEnergySettings> { }


    [MessagePackObject]
    public class UserEnergySettingsMapper : NoChildSettingsMapper<UserEnergySettings> { }
}
