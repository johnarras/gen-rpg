using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.TimeOfDay.Settings
{
    [MessagePackObject]
    public class StatRegenHours
    {
        [Key(0)] public long StatTypeId { get; set; }
        [Key(1)] public double RegenHours { get; set; }
        [Key(2)] public string Name { get; set; } = null;
    }

    [MessagePackObject]
    public class TimeOfDaySettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double DailyResetHour { get; set; } = 8;
        [Key(2)] public double BaseMoveMinutes { get; set; } = 1;
        [Key(3)] public double CombatRoundMinutes { get; set; } = 1;
        [Key(4)] public double RestHours { get; set; } = 8;

        [Key(5)] public List<StatRegenHours> RegenHours { get; set; }
    }


    [MessagePackObject]
    public class TimeOfDaySettingsLoader : NoChildSettingsLoader<TimeOfDaySettings> { }



    [MessagePackObject]
    public class TimeOfDaySettingsMapper : NoChildSettingsMapper<TimeOfDaySettings> { }
}
