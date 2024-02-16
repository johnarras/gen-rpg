using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.Shared.Core.Settings
{
    [MessagePackObject]
    public class CoreSettings : TopLevelGameSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string GameName { get; set; }
        [Key(2)] public string GameVersion { get; set; }
        [Key(3)] public string UnityProjectId { get; set; }
        [Key(4)] public string BundleId { get; set; }
    }


    [MessagePackObject]
    public class CoreSettingsLoader : GameSettingsLoader<CoreSettings> { }
}
