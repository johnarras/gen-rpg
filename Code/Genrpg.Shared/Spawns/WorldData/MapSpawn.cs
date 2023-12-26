using MessagePack;

using System;
using System.Linq;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.WorldData;
using System.Collections.Generic;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Newtonsoft.Json;

namespace Genrpg.Shared.Spawns.WorldData
{
    [MessagePackObject]
    public class MapSpawn : BaseWorldData, IMapSpawn, IMapOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string ObjId { get; set; }
        [Key(2)] public string OwnerId { get; set; }
        [Key(3)] public string MapId { get; set; }
        [Key(4)] public DateTime LastSpawnTime { get; set; }
        [Key(5)] public long EntityTypeId { get; set; }
        [Key(6)] public long EntityId { get; set; }
        [Key(7)] public string Name { get; set; }
        [Key(8)] public float X { get; set; }
        [Key(9)] public float Z { get; set; }
        [Key(10)] public short Rot { get; set; }
        [Key(11)] public long ZoneId { get; set; }
        [Key(12)] public int SpawnSeconds { get; set; }
        [Key(13)] public int OverrideZonePercent { get; set; }
        [Key(14)] public long FactionTypeId { get; set; }
        [Key(15)] public string AddonString { get; set; } // TODO: better system than this hack
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(16)] public List<IMapObjectAddon> Addons { get; set; } = new List<IMapObjectAddon>();
        public List<IMapObjectAddon> GetAddons()
        {
            if (Addons == null)
            {
                Addons = new List<IMapObjectAddon>();
            }

            if (Addons.Count > 0)
            {
                return Addons;
            }
            return Addons;
        }

        public long GetAddonBits()
        {
            long addonBits = 0;
            List<IMapObjectAddon> addons = GetAddons();

            foreach (IMapObjectAddon addon in addons)
            {
                addonBits |= (long)(1 << (int)addon.GetAddonType());
            }
            return addonBits;
        }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
    }
}
