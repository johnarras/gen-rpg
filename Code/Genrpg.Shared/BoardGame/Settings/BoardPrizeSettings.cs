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
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.Shared.BoardGame.Settings
{

    [MessagePackObject]
    public class PrizeSpawn
    {
        [Key(0)] public long BoardPrizeId { get; set; }
        [Key(1)] public double Weight { get; set; }
    }


    [MessagePackObject]
    public class BoardPrizeSettings : ParentSettings<BoardPrize>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class BoardPrize : ChildSettings, IIdName
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public long VisualEntityTypeId { get; set; } // What it looks like
        [Key(5)] public long VisualEntityId { get; set; } // What it looks like
        [Key(6)] public List<SpawnItem> LootTable { get; set; } = new List<SpawnItem>(); // What it actually spawns
    }

    [MessagePackObject]
    public class BoardPrizeSettingsApi : ParentSettingsApi<BoardPrizeSettings, BoardPrize> { }

    [MessagePackObject]
    public class BoardPrizeSettingsLoader : ParentSettingsLoader<BoardPrizeSettings, BoardPrize> { }

    [MessagePackObject]
    public class BoardPrizeSettingsMapper : ParentSettingsMapper<BoardPrizeSettings, BoardPrize, BoardPrizeSettingsApi> { }
}
