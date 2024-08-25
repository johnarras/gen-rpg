using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Users.Settings
{
    [MessagePackObject]
    public class NewUserSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long StartCreditsMult { get; set; } = 10;

        [Key(2)] public long StartGems { get; set; } = 50;
        [Key(3)] public long StartEnergy { get; set; } = 100;
        [Key(4)] public long StartCredits { get; set; } = 250;
    }


    [MessagePackObject]
    public class NewUserSettingsLoader : NoChildSettingsLoader<NewUserSettings> { }

    [MessagePackObject]
    public class NewSettingsMapper : NoChildSettingsMapper<NewUserSettings> { }
}
