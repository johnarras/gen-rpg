using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class InputSettings : ParentSettings<ActionInputSetting>
    {
        [Key(0)] public override string Id { get; set; }
    }


    [MessagePackObject]
    public class ActionInputSetting : ChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public int Index { get; set; }
        [Key(3)] public long SpellId { get; set; }
        [Key(4)] public override string Name { get; set; }
    }

    [MessagePackObject]
    public class ActionInputSettingsApi : ParentSettingsApi<InputSettings, ActionInputSetting> { }
    [MessagePackObject]
    public class ActionInputSettingsLoader : ParentSettingsLoader<InputSettings, ActionInputSetting,ActionInputSettingsApi> { }

}
