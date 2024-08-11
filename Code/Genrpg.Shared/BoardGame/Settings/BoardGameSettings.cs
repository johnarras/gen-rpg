using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;
using Genrpg.Shared.Achievements.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.BoardGame.Settings
{

    [MessagePackObject]
    public class BoardGameSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double RollGoldMult { get; set; } = 25;
        [Key(2)] public double GemRollGoldFraction { get; set; } = 0.5; // A gem equals half a roll worth of gold.

    }

    [MessagePackObject]
    public class BoardGameSettingsLoader : NoChildSettingsLoader<BoardGameSettings> { }

    [MessagePackObject]
    public class BoardGameSettingsMapper : NoChildSettingsMapper<BoardGameSettings> { }
}
