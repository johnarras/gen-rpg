using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Dungeons.Constants;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack;
using System.Collections.Generic;
using System.Transactions;

namespace Genrpg.Shared.Crawler.Maps.Settings
{
    [MessagePackObject]
    public class MapEncounterSettings : ParentSettings<MapEncounterType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double EncounterChance { get; set; }
    }

    [MessagePackObject]
    public class MapEncounterType : ChildSettings, IIndexedGameItem, IWeightedItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public double Weight { get; set; }
        [Key(8)] public bool CanRepeat { get; set; }

    }

    [MessagePackObject]
    public class MapEncounterSettingsApi : ParentSettingsApi<MapEncounterSettings, MapEncounterType> { }
    [MessagePackObject]
    public class MapEncounterSettingsLoader : ParentSettingsLoader<MapEncounterSettings, MapEncounterType> { }

    [MessagePackObject]
    public class MapEncounterSettingsMapper : ParentSettingsMapper<MapEncounterSettings, MapEncounterType, MapEncounterSettingsApi> { }

}
