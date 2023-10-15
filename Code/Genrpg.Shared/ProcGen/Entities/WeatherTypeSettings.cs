using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class WeatherTypeSettings : ParentSettings<WeatherType>
    {
        [Key(0)] public override string Id { get; set; }

        public WeatherType GetWeatherType(long idkey) { return _lookup.Get<WeatherType>(idkey); }
    }

    [MessagePackObject]
    public class WeatherTypeSettingsApi : ParentSettingsApi<WeatherTypeSettings, WeatherType> { }
    [MessagePackObject]
    public class WeatherTypeSettingsLoader : ParentSettingsLoader<WeatherTypeSettings, WeatherType, WeatherTypeSettingsApi> { }

}
