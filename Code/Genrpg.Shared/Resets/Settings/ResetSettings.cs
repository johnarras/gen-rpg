using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Resets.Settings
{
    [MessagePackObject]
    public class ResetSettings : TopLevelGameSettings
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double ResetHour { get; set; }
    }
}
