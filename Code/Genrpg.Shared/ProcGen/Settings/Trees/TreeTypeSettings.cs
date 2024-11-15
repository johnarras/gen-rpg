using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.ProcGen.Settings.Trees;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Trees
{
    [MessagePackObject]
    public class TreeTypeSettings : ParentSettings<TreeType>
    {
        [Key(0)] public override string Id { get; set; }


        [Key(1)] public float TallChance { get; set; } = 0.5f;
        [Key(2)] public float TreeDirtRadius { get; set; } = 9.0f;
    }

    [MessagePackObject]
    public class TreeTypeSettingsApi : ParentSettingsApi<TreeTypeSettings, TreeType> { }
    [MessagePackObject]
    public class TreeTypeSettingsLoader : ParentSettingsLoader<TreeTypeSettings, TreeType> { }

    [MessagePackObject]
    public class TreeSettingsMapper : ParentSettingsMapper<TreeTypeSettings, TreeType, TreeTypeSettingsApi> { }


}
