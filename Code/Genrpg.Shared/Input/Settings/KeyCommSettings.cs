using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Input.Settings
{
    [MessagePackObject]
    public class KeyCommSettings : ParentSettings<KeyCommSetting>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class KeyCommSetting : ChildSettings, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long IdKey { get; set; }
        [Key(2)] public override string Name { get; set; }
        [Key(3)] public override string ParentId { get; set; }
        [Key(4)] public string KeyPress { get; set; }
        [Key(5)] public string KeyCommand { get; set; }
        [Key(6)] public int Modifiers { get; set; }
    }

    [MessagePackObject]
    public class KeyCommSettingsApi : ParentSettingsApi<KeyCommSettings, KeyCommSetting> { }
    [MessagePackObject]
    public class KeyCommSettingsLoader : ParentSettingsLoader<KeyCommSettings, KeyCommSetting> { }

    [MessagePackObject]
    public class KeyCommSettingsMapper : ParentSettingsMapper<KeyCommSettings, KeyCommSetting, KeyCommSettingsApi> { }


}
