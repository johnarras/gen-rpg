using MessagePack;
using Genrpg.Shared.DataStores.Categories;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Spawns.Entities
{

    [MessagePackObject]
    public class MapSpawnData : BaseWorldData, IStringOwnerId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public string OwnerId { get; set; }
        [Key(2)] public List<MapSpawn> Data { get; set; }
        [Key(3)] public int MaxId { get; set; } = 12345;
        [Key(4)] public List<NPCStatus> NPCs { get; set; }

        public MapSpawnData()
        {
            Data = new List<MapSpawn>();
            NPCs = new List<NPCStatus>();
        }

        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }

        public void AddSpawn(long entityTypeId, long entityId, int mapx, int mapz, long zoneId)
        {
            MapSpawn currSpawn = Data.FirstOrDefault(sp => sp.X == mapx && sp.Z == mapz);
            if (currSpawn != null)
            {
                currSpawn.EntityTypeId = entityTypeId;
                currSpawn.EntityId = entityId;
                return;
            }

            MaxId++;

            List<char> idChars = HashUtils.GetIdChars();

            string strId = "";

            if (idChars.Count < 1)
            {
                strId = MaxId.ToString();
            }
            else
            {
                int idval = MaxId;

                while (idval > 0)
                {
                    strId += idChars[idval % idChars.Count];
                    idval /= idChars.Count;
                }
                strId.Reverse();
            }


            MapSpawn spawn = new MapSpawn()
            {
                MapObjectId = strId,
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                X = mapx,
                Z = mapz,
                ZoneId = zoneId,
                SpawnSeconds = SpawnConstants.DefaultSpawnSeconds,
            };
            Data.Add(spawn);

        }
    }
}
