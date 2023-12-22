using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
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

        public TreeType GetTreeType(long idkey) { return _lookup.Get<TreeType>(idkey); }
    }

    [MessagePackObject]
    public class TreeTypeSettingsApi : ParentSettingsApi<TreeTypeSettings, TreeType> { }
    [MessagePackObject]
    public class TreeTypeSettingsLoader : ParentSettingsLoader<TreeTypeSettings, TreeType, TreeTypeSettingsApi> { }

}
