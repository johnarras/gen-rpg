using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Versions.Settings
{
    [MessagePackObject]
    public class VersionSettings : BaseGameSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int ClientVersion { get; set; }
        [Key(2)] public int ServerVersion { get; set; }
        [Key(3)] public DateTime GameDataSaveTime { get; set; }
        [Key(4)] public int UserVersion { get; set; }
        [Key(5)] public int CharacterVersion { get; set; }
    }

    [MessagePackObject]
    public class VersionSettingsLoader : GameSettingsLoader<VersionSettings> { }
}
