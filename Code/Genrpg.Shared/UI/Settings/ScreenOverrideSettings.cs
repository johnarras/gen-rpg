using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UI.Settings
{
    [MessagePackObject]
    public class ScreenOverride : ChildSettings, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NewName { get; set; }

    }
    [MessagePackObject]
    public class ScreenOverrideSettings : ParentSettings<ScreenOverride>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ScreenOverrideSettingsApi : ParentSettingsApi<ScreenOverrideSettings, ScreenOverride> { }
    [MessagePackObject]
    public class ScreenOverrideSettingsLoader : ParentSettingsLoader<ScreenOverrideSettings, ScreenOverride, ScreenOverrideSettingsApi> { }
}
