using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Settings.Settings
{
    [MessagePackObject]
    public class SettingsName : ChildSettings, IIdName
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }


        public SettingsName()
        {
        }
    }

    [MessagePackObject]
    public class SettingsNameSettings : ParentSettings<SettingsName>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class SettingsNameSettingsApi : ParentSettingsApi<SettingsNameSettings, SettingsName> { }

    [MessagePackObject]
    public class SettingsNameSettingsLoader : ParentSettingsLoader<SettingsNameSettings, SettingsName> { }

    [MessagePackObject]
    public class SettingsNameSettingsMapper : ParentSettingsMapper<SettingsNameSettings, SettingsName, SettingsNameSettingsApi> { }

}
