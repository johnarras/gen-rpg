using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.DataStores.Categories.WorldData;
using Genrpg.Shared.Spawns.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Core.Entities;

namespace Genrpg.Shared.Spawns.WorldData
{



    [MessagePackObject]
    public class MapSpawnData : BaseWorldData, IStringOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public List<MapSpawn> Data { get; set; } = new List<MapSpawn>();
        [Key(3)] public int MaxId { get; set; } = 12345;

        public override void Delete(IRepositoryService repoSystem) { repoSystem.Delete(this); }

        public void AddSpawn(InitSpawnData initSpawnData)
        {
            MapSpawn currSpawn = Data.FirstOrDefault(sp => sp.X == initSpawnData.SpawnX && sp.Z == initSpawnData.SpawnZ);
            if (currSpawn != null)
            {
                currSpawn.EntityTypeId = initSpawnData.EntityTypeId;
                currSpawn.EntityId = initSpawnData.EntityId;
                return;
            }

            string strId = HashUtils.GetIdFromVal(MaxId++);

            MapSpawn spawn = new MapSpawn()
            {
                ObjId = strId,
                EntityTypeId = initSpawnData.EntityTypeId,
                EntityId = initSpawnData.EntityId,
                X = initSpawnData.SpawnX,
                Z = initSpawnData.SpawnZ,
                Rot = (short)initSpawnData.Rot,
                ZoneId = initSpawnData.ZoneId,
                LocationId = initSpawnData.LocationId,
                LocationPlaceId = initSpawnData.LocationPlaceId,
                SpawnSeconds = initSpawnData.SpawnSeconds,
                OverrideZonePercent = initSpawnData.ZoneOverridePercent,
                Addons = initSpawnData.Addons,
                Name = initSpawnData.Name,
            };
            Data.Add(spawn);

        }
    }
}
