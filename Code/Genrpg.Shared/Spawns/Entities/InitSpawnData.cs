using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Spawns.Constants;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spawns.Entities
{
    // Used to set up a spawn, not exact same object to allow us to add/remove extra data relative to the final spawn.
    [MessagePackObject]
    public class InitSpawnData
    {
        [Key(0)] public long EntityTypeId { get; set; }
        [Key(1)] public long EntityId { get; set; }
        [Key(2)] public string Name { get; set; }
        [Key(3)] public float SpawnX { get; set; }
        [Key(4)] public float SpawnZ { get; set; }
        [Key(5)] public float Rot { get; set; }
        [Key(6)] public long ZoneId { get; set; }
        [Key(7)] public long FactionTypeId { get; set; } = 1;
        [Key(8)] public int ZoneOverridePercent { get; set; }
        [Key(9)] public int SpawnSeconds { get; set; } = SpawnConstants.DefaultSpawnSeconds;
        [Key(10)] public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
    }
}
