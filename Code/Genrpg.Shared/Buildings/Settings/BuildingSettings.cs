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

namespace Genrpg.Shared.Buildings.Settings
{
    [MessagePackObject]
    public class BuildingSettings : ParentSettings<BuildingType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class BuildingType : ChildSettings, IVariationIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int Radius { get; set; } = 5;

        [Key(8)] public bool IsCrawlerBuilding { get; set; }
        [Key(9)] public int VariationCount { get; set; } = 1;
    }

    [MessagePackObject]
    public class BuildingSettingsApi : ParentSettingsApi<BuildingSettings, BuildingType> { }

    [MessagePackObject]
    public class BuildingSettingsLoader : ParentSettingsLoader<BuildingSettings, BuildingType> { }

    [MessagePackObject]
    public class BuildingSettingsMapper : ParentSettingsMapper<BuildingSettings, BuildingType, BuildingSettingsApi> { }
}
