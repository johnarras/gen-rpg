using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Spawns.WorldData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace Genrpg.ServerShared.MapSpawns
{

    public interface IMapSpawnDataService : ISetupService
    {
        Task SaveMapSpawnData(ServerGameState gs, MapSpawnData data, string mapId, int mapVersion);
        Task<MapSpawnData> LoadMapSpawnData(GameState gs, string mapId, int mapVersion);
    }

    public class MapSpawnDataService : IMapSpawnDataService
    {
        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { MemberName = "OwnerId" });
            configs.Add(new IndexConfig() { MemberName = "MapId" });
            List<Task> allTasks = new List<Task>();
            allTasks.Add(gs.repo.CreateIndex<UnitStatus>(configs));
            allTasks.Add(gs.repo.CreateIndex<MapSpawn>(configs));
            await Task.WhenAll(allTasks);
        }
        public async Task SaveMapSpawnData(ServerGameState gs, MapSpawnData data, string mapId, int mapVersion)
        {
            await gs.repo.DeleteAll<MapSpawn>(x => x.MapId == mapId);
            string ownerId = Map.GetMapOwnerId(mapId, mapVersion);
            foreach (MapSpawn spawn in data.Data)
            {
                spawn.Id = spawn.ObjId + "-" + ownerId;
                spawn.OwnerId = ownerId;
                spawn.MapId = mapId;
                if (spawn.Addons != null)
                {
                    spawn.AddonString = SerializationUtils.Serialize(spawn.Addons);
                    spawn.Addons = null;
                }
            }

            await gs.repo.SaveAll(data.Data);

        }

        public async Task<MapSpawnData> LoadMapSpawnData(GameState gs, string mapId, int mapVersion)
        {
            MapSpawnData spawnData = new MapSpawnData();

            string mapOwnerId = Map.GetMapOwnerId(mapId, mapVersion);

            spawnData.Data = await gs.repo.Search<MapSpawn>(x => x.OwnerId == mapOwnerId, 1000000);

            foreach (MapSpawn mapSpawn in spawnData.Data)
            {
                if (!string.IsNullOrEmpty(mapSpawn.AddonString))
                {
                    mapSpawn.Addons = SerializationUtils.Deserialize<List<IMapObjectAddon>>(mapSpawn.AddonString);
                    mapSpawn.AddonString = null;
                }
            }


            return spawnData;
        }

    }
}
