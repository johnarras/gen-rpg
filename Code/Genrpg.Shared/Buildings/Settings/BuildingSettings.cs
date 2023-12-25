using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Vendors.WorldData;

namespace Genrpg.Shared.Buildings.Settings
{
    [MessagePackObject]
    public class BuildingSettings : ParentSettings<BuildingType>
    {
        [Key(0)] public override string Id { get; set; }

        public BuildingType GetBuildingType(long idkey) { return _lookup.Get<BuildingType>(idkey); }
    }

    [MessagePackObject]
    public class BuildingType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int Radius { get; set; } = 2;

    }

    [MessagePackObject]
    public class BuildingSettingsApi : ParentSettingsApi<BuildingSettings, BuildingType> { }

    [MessagePackObject]
    public class BuildingSettingsLoader : ParentSettingsLoader<BuildingSettings, BuildingType, BuildingSettingsApi> { }

}
