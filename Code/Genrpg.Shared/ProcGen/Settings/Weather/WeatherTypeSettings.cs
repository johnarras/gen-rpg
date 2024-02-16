using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Weather
{
    [MessagePackObject]
    public class WeatherTypeSettings : ParentSettings<WeatherType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class WeatherTypeSettingsApi : ParentSettingsApi<WeatherTypeSettings, WeatherType> { }
    [MessagePackObject]
    public class WeatherTypeSettingsLoader : ParentSettingsLoader<WeatherTypeSettings, WeatherType, WeatherTypeSettingsApi> { }

}
