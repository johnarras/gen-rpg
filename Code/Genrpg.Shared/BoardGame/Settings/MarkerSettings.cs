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
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class MarkerSettings : ParentSettings<Marker>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<MarkerTier> Tiers { get; set; } = new List<MarkerTier>();
    }

    [MessagePackObject]
    public class MarkerTier
    {
        [Key(0)] public long Tier { get; set; } = 1;
        [Key(1)] public long MinQuantity { get; set; }
    }

    [MessagePackObject]
    public class Marker : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int MaxTier { get; set; }
        [Key(8)] public long QualityTypeId { get; set; }
    }

    [MessagePackObject]
    public class MarkerSettingsApi : ParentSettingsApi<MarkerSettings, Marker> { }

    [MessagePackObject]
    public class MarkerSettingsLoader : ParentSettingsLoader<MarkerSettings, Marker> { }

    [MessagePackObject]
    public class MarkerSettingsMapper : ParentSettingsMapper<MarkerSettings, Marker, MarkerSettingsApi> { }
}
