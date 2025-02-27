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
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Tiles.Settings
{
    [MessagePackObject]
    public class TileTypeSettings : ParentSettings<TileType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TileType : ChildSettings, IIndexedGameItem, IWeightedItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public double Weight { get; set; }

        [Key(8)] public List<SpawnItem> PassRewards { get; set; } = new List<SpawnItem>();

        // If this has an ActivationCostScale > 0, do not give on all lands, instead wait for chargeup.
        [Key(9)] public List<SpawnItem> LandRewards { get; set; } = new List<SpawnItem>();
    }

    [MessagePackObject]
    public class TileTypeSettingsApi : ParentSettingsApi<TileTypeSettings, TileType> { }

    [MessagePackObject]
    public class TileTypeSettingsLoader : ParentSettingsLoader<TileTypeSettings, TileType> { }

    [MessagePackObject]
    public class TileTypeSettingsMapper : ParentSettingsMapper<TileTypeSettings, TileType, TileTypeSettingsApi> { }
}
