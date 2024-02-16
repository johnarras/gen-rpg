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

namespace Genrpg.Shared.Spawns.Settings
{
    [MessagePackObject]
    public class SpawnItem
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long MinQuantity { get; set; }
        [Key(3)] public long MaxQuantity { get; set; }
        [Key(4)] public double Weight { get; set; }
        [Key(5)] public int GroupId { get; set; }

        public SpawnItem()
        {
            MinQuantity = 1;
            MaxQuantity = 1;
            Weight = 100.0;
        }
    }
    [MessagePackObject]
    public class SpawnSettings : ParentSettings<SpawnTable>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public float MapSpawnChance { get; set; }
        [Key(2)] public long MonsterLootSpawnTableId { get; set; }
    }
    [MessagePackObject]
    public class SpawnTable : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public List<SpawnItem> Items { get; set; }
        [Key(8)] public string Art { get; set; }

        public SpawnTable()
        {
            Items = new List<SpawnItem>();
        }

    [MessagePackObject]
        public class SpawnSettingsApi : ParentSettingsApi<SpawnSettings, SpawnTable> { }
    [MessagePackObject]
        public class SpawnSettingsLoader : ParentSettingsLoader<SpawnSettings, SpawnTable, SpawnSettingsApi> { }


    }
}
