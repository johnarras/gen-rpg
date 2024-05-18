using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Zones.Settings
{
    [MessagePackObject]
    public class ZoneTypeSettings : ParentSettings<ZoneType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ZoneTypeSettingsApi : ParentSettingsApi<ZoneTypeSettings, ZoneType> { }
    [MessagePackObject]
    public class ZoneTypeSettingsLoader : ParentSettingsLoader<ZoneTypeSettings, ZoneType> { }

    [MessagePackObject]
    public class ZoneTypeSettingsMapper : ParentSettingsMapper<ZoneTypeSettings, ZoneType, ZoneTypeSettingsApi> { }

}
