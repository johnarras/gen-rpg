using Genrpg.ServerShared.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.MapObjectAddons.Entities;
using Genrpg.Shared.MapServer.Constants;
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
            await gs.repo.CreateIndex<UnitStatus>(configs);
            await gs.repo.CreateIndex<MapSpawn>(configs);
        }
        public async Task SaveMapSpawnData(ServerGameState gs, MapSpawnData data, string mapOwnerId)
        {
            foreach (MapSpawn spawn in data.Data)
            {
                spawn.Id = spawn.ObjId + "-" + mapOwnerId;
                spawn.OwnerId = mapOwnerId;
                if (spawn.Addons != null)
                {
                    spawn.AddonString = SerializationUtils.Serialize(spawn.Addons);
                    spawn.Addons = null;
                }
            }

            await gs.repo.SaveAll(data.Data);

        }

        public async Task<MapSpawnData> LoadMapSpawnData(GameState gs, string mapOwnerId)
        {
            MapSpawnData spawnData = new MapSpawnData();

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
