using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Resets.Settings
{
    [MessagePackObject]
    public class ResetSettings : NoChildSettings
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double ResetHour { get; set; }
    }
    [MessagePackObject]
    public class ResetSettingsLoader : NoChildSettingsLoader<ResetSettings> { }


    [MessagePackObject]
    public class ResetSettingsMapper : NoChildSettingsMapper<ResetSettings> { }
}
