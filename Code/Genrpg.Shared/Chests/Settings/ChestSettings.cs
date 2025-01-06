using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using System.Collections.Generic;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Chests.Settings
{
    [MessagePackObject]
    public class ChestSettings : ParentSettings<Chest>
    {
        [Key(0)] public override string Id { get; set; }

        /// <summary>
        /// Base loot with scaling for tiered chests.
        /// </summary>
        [Key(1)] public List<SpawnItem> TieredCurrencyLoot { get; set; } = new List<SpawnItem>();
    }

    [MessagePackObject]
    public class Chest : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long UnlockMinutes { get; set; }

        [Key(8)] public int Tier { get; set; } // Is this a tiered chest?

        [Key(9)] public int TieredLootMult { get; set; } // Loot Mult for this tiered chest.

        [Key(10)] public List<SpawnItem> Loot { get; set; } = new List<SpawnItem>();


    }

    [MessagePackObject]
    public class ChestSettingsApi : ParentSettingsApi<ChestSettings, Chest> { }

    [MessagePackObject]
    public class ChestSettingsLoader : ParentSettingsLoader<ChestSettings, Chest> { }

    [MessagePackObject]
    public class ChestSettingsMapper : ParentSettingsMapper<ChestSettings, Chest, ChestSettingsApi> { }


    public class ChestHelper : BaseEntityHelper<ChestSettings, Chest>
    {
        public override long GetKey() { return EntityTypes.Chest; }
    }
}
