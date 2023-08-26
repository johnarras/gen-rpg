using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Constants;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Quests.Entities;
using Genrpg.Shared.Spawns.Entities;
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

    public interface IMapSpawnService : ISetupService
    {
        Task SaveMapSpawnData(ServerGameState gs, MapSpawnData data, string mapOwnerId);
        Task<MapSpawnData> LoadMapSpawnData(GameState gs, string mapOwnerId);
    }

    public class MapSpawnService : IMapSpawnService
    {
        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { MemberName = "OwnerId" });
            await gs.repo.CreateIndex<NPCStatus>(configs);
            await gs.repo.CreateIndex<MapSpawn>(configs);
        }
        public async Task SaveMapSpawnData(ServerGameState gs, MapSpawnData data, string mapOwnerId)
        {
            MapSpawnData[] spawnArray = new MapSpawnData[SharedMapConstants.MapSpawnArraySize];

            foreach (NPCStatus status in data.NPCs)
            {
                status.Id = status.MapObjectId + "-" + mapOwnerId;
                status.OwnerId = mapOwnerId;
            }

            await gs.repo.SaveAll(data.NPCs);

            foreach (MapSpawn spawn in data.Data)
            {
                spawn.Id = spawn.MapObjectId + "-" + mapOwnerId;
                spawn.OwnerId = mapOwnerId;
            }

            await gs.repo.SaveAll(data.Data);

        }

        public async Task<MapSpawnData> LoadMapSpawnData(GameState gs, string mapOwnerId)
        {
            MapSpawnData spawnData = new MapSpawnData();


            spawnData.NPCs = await gs.repo.Search<NPCStatus>(x => x.OwnerId == mapOwnerId, 1000000);

            spawnData.Data = await gs.repo.Search<MapSpawn>(x => x.OwnerId == mapOwnerId, 1000000);

            return spawnData;
        }

    }
}
