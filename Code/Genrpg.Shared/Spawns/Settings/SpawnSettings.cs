using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Spawns.Settings
{
    public interface ISpawnItem
    {
        long EntityTypeId { get; }
        long EntityId { get; }
        long MinQuantity { get; }
        long MaxQuantity { get; }
        double Weight { get; }
        int GroupId { get;}
        string Name { get; }
        long MinLevel { get;}
        double MinScale { get; }
    }


    [MessagePackObject]
    public class SpawnItem : ISpawnItem
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public long MinQuantity { get; set; } = 1;
        [Key(3)] public long MaxQuantity { get; set; } = 1;
        [Key(4)] public double Weight { get; set; } = 100;
        [Key(5)] public int GroupId { get; set; }
        [Key(6)] public string Name { get; set; }
        [Key(7)] public long MinLevel { get; set; }
        [Key(8)] public double MinScale { get; set; }

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
        public class SpawnSettingsLoader : ParentSettingsLoader<SpawnSettings, SpawnTable> { }

    [MessagePackObject]
        public class SpawnSettingsMapper : ParentSettingsMapper<SpawnSettings, SpawnTable, SpawnSettingsApi> { }


    }
}
