using MessagePack;

using System;
using System.Linq;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;

namespace Genrpg.Shared.Spawns.Entities
{
    [MessagePackObject]
    public class MapSpawn : BaseWorldData, IMapSpawn, IStringOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string MapObjectId { get; set; }
        [Key(2)] public string OwnerId { get; set; }
        [Key(3)] public DateTime LastSpawnTime { get; set; }
        [Key(4)] public long EntityTypeId { get; set; }
        [Key(5)] public long EntityId { get; set; }
        [Key(6)] public float X { get; set; }
        [Key(7)] public float Z { get; set; }
        [Key(8)] public long ZoneId { get; set; }
        [Key(9)] public int SpawnSeconds { get; set; }
        [Key(10)] public long FactionTypeId { get; set; }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Save(this); }
    }
}
