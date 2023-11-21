using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Core.Entities
{
    [MessagePackObject]
    public class CoreSettings : BaseGameSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string Env { get; set; }
        [Key(2)] public string GameName { get; set; }
        [Key(3)] public string ArtURL { get; set; }
        [Key(4)] public string GameVersion { get; set; }
        [Key(5)] public string UnityProjectId { get; set; }
        [Key(6)] public string BundleId { get; set; }
    }


    [MessagePackObject]
    public class CoreSettingsLoader : GameSettingsLoader<CoreSettings> { }
}
