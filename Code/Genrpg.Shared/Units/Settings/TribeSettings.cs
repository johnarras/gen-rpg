using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Units.Settings
{
    [MessagePackObject]
    public class TribeSettings : ParentSettings<TribeType>
    {
        [Key(0)] public override string Id { get; set; }

    }
    [MessagePackObject]
    public class TribeType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public List<SpawnItem> LootItems { get; set; } = new List<SpawnItem>();
        [Key(8)] public List<SpawnItem> InteractLootItems { get; set; } = new List<SpawnItem>();
        [Key(9)] public long LootCrafterTypeId { get; set; }

    [MessagePackObject]
        public class TribeSettingsApi : ParentSettingsApi<TribeSettings, TribeType> { }

    [MessagePackObject]
        public class TribeSettingsLoasder : ParentSettingsLoader<TribeSettings, TribeType> { }

    [MessagePackObject]
        public class TribeTypeSettingsMapper : ParentSettingsMapper<TribeSettings, TribeType, TribeSettingsApi> { }
    }
}
