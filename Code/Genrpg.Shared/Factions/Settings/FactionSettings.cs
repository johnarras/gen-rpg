using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Factions.Settings
{
    [MessagePackObject]
    public class FactionSettings : ParentSettings<FactionType>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class FactionType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public int StartRepLevelId { get; set; }

    }

    [MessagePackObject]
    public class FactionSettingsApi : ParentSettingsApi<FactionSettings, FactionType> { }

    [MessagePackObject]
    public class FactionSettingsLoader : ParentSettingsLoader<FactionSettings,FactionType> { }

    [MessagePackObject]
    public class FactionSettingsMapper : ParentSettingsMapper<FactionSettings, FactionType, FactionSettingsApi> { }

}
