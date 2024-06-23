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

namespace Genrpg.Shared.Biomes.Settings
{
    [MessagePackObject]
    public class BiomeSettings : ParentSettings<BiomeType>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class BiomeType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }

        [Key(7)] public int MinLevel { get; set; }


        [Key(8)] public string BaseTileName { get; set; }
        [Key(9)] public string WaterTileName { get; set; }
        [Key(10)] public string TreeObjectName { get; set; }
        [Key(11)] public string BushObjectName { get; set; }
    }


    [MessagePackObject]
    public class BiomeSettingsApi : ParentSettingsApi<BiomeSettings, BiomeType> { }

    [MessagePackObject]
    public class BiomeSettingsLoader : ParentSettingsLoader<BiomeSettings,BiomeType> { }

    [MessagePackObject]
    public class BiomeSettingsMapper : ParentSettingsMapper<BiomeSettings, BiomeType, BiomeSettingsApi> { }

}
