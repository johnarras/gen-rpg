using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Purchasing.Settings
{
    [MessagePackObject]
    public class StoreThemeSettings : ParentSettings<StoreTheme>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StoreThemeSettingsApi : ParentSettingsApi<StoreThemeSettings, StoreTheme> { }
    [MessagePackObject]
    public class StoreThemeSettingsLoader : ParentSettingsLoader<StoreThemeSettings, StoreTheme> { }

    [MessagePackObject]
    public class StoreThemeSettingsMapper : ParentSettingsMapper<StoreThemeSettings, StoreTheme, StoreThemeSettingsApi> { }


    [MessagePackObject]
    public class StoreTheme : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Icon { get; set; }
    }
}
