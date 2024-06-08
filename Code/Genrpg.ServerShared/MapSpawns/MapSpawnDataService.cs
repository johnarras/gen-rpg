using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
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

    public interface IMapSpawnDataService : IInitializable
    {
        Task SaveMapSpawnData(IRepositoryService repoService, MapSpawnData data, string mapId, int mapVersion);
        Task<MapSpawnData> LoadMapSpawnData(IRepositoryService repoService, string mapId, int mapVersion);
    }

    public class MapSpawnDataService : IMapSpawnDataService
    {
        public async Task Initialize(IGameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { MemberName = nameof(UnitStatus.OwnerId) });
            configs.Add(new IndexConfig() { MemberName = nameof(UnitStatus.MapId) });
            List<Task> allTasks = new List<Task>();
            IRepositoryService repoService = gs.loc.Get<IRepositoryService>();
            allTasks.Add(repoService.CreateIndex<UnitStatus>(configs));
            allTasks.Add(repoService.CreateIndex<MapSpawn>(configs));
            await Task.WhenAll(allTasks);
        }
        public async Task SaveMapSpawnData(IRepositoryService repoService, MapSpawnData data, string mapId, int mapVersion)
        {
            await repoService.DeleteAll<MapSpawn>(x => x.MapId == mapId);
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

            await repoService.SaveAll(data.Data);

        }

        public async Task<MapSpawnData> LoadMapSpawnData(IRepositoryService repoService, string mapId, int mapVersion)
        {
            MapSpawnData spawnData = new MapSpawnData();

            string mapOwnerId = Map.GetMapOwnerId(mapId, mapVersion);

            spawnData.Data = await repoService.Search<MapSpawn>(x => x.OwnerId == mapOwnerId, 1000000);

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
