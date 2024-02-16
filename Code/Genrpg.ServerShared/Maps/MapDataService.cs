using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Maps.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.WorldData;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Maps
{
    public interface IMapDataService : ISetupService
    {
        Task<List<MapStub>> GetMapStubs(ServerGameState gs);
        Task<Map> LoadMap(ServerGameState gs, string mapId);
        Task SaveMap(ServerGameState gs, Map map);
    }

    public class MapDataService : IMapDataService
    {
        public async Task Setup(GameState gs, CancellationToken token)
        {
            List<IndexConfig> configs = new List<IndexConfig>();
            configs.Add(new IndexConfig() { MemberName = "OwnerId" });
            configs.Add(new IndexConfig() { MemberName = "MapId" });
            List<Task> allTasks = new List<Task>();
            allTasks.Add(gs.repo.CreateIndex<QuestType>(configs));
            allTasks.Add(gs.repo.CreateIndex<QuestItem>(configs));
            allTasks.Add(gs.repo.CreateIndex<Zone>(configs));
        }

        public async Task<List<MapStub>> GetMapStubs(ServerGameState gs)
        {
            List<MapRoot> allMaps = await gs.repo.Search<MapRoot>(x => true);

            List<MapStub> allStubs = new List<MapStub>();

            foreach (MapRoot map in allMaps)
            {
                MapStub stub = new MapStub();
                stub.CopyFrom(map);
                allStubs.Add(stub);
            }
            return allStubs;
        }

        public async Task<Map> LoadMap(ServerGameState gs, string mapId)
        {
            MapRoot root = await gs.repo.Load<MapRoot>(mapId);

            if (root == null)
            {
                root = new MapRoot()
                {
                    Id = mapId,
                    MapVersion = 0
                };
                await gs.repo.Save(root);
            }

            Map map = SerializationUtils.Deserialize<Map>(SerializationUtils.Serialize(root));

            string mapOwnerId = Map.GetMapOwnerId(map);

            map.Zones = await LoadMapDataList<Zone>(gs, mapOwnerId);
            map.Quests = await LoadMapDataList<QuestType>(gs, mapOwnerId);
            map.QuestItems = await LoadMapDataList<QuestItem>(gs, mapOwnerId);

            return map;
        }

        public async Task SaveMap(ServerGameState gs, Map map)
        {

            try
            {
                MapRoot mapRoot = SerializationUtils.Deserialize<MapRoot>(SerializationUtils.Serialize(map));

                // Do not save map. It's too big.
                await gs.repo.Save(mapRoot);

                string mapOwnerId = Map.GetMapOwnerId(map);

                await SaveMapDataList(gs, map.Zones, map.Id, map.MapVersion);
                await SaveMapDataList(gs, map.Quests, map.Id, map.MapVersion);
                await SaveMapDataList(gs, map.QuestItems, map.Id, map.MapVersion);
            }
            catch (Exception ex)
            {
                gs.logger.Exception(ex, "SaveMap");
            }
        }

        protected async Task SaveMapDataList<T>(ServerGameState gs, List<T> list, string mapId, int mapVersion) where T : class, IMapOwnerId, IId
        {
            await gs.repo.DeleteAll<T>(x => x.MapId == mapId);
            string ownerId = Map.GetMapOwnerId(mapId, mapVersion);
            foreach (T t in list)
            {
                t.OwnerId = ownerId;
                t.MapId = mapId;
                t.Id = t.IdKey + "-" + ownerId;
            }
            await gs.repo.SaveAll(list);
        }

        protected async Task<List<T>> LoadMapDataList<T>(ServerGameState gs, string ownerId) where T : class, IStringOwnerId
        {
            List<T> retval = new List<T>();

            int quantity = 10000;
            int skip = 0;

            while (true)
            {
                List<T> newList = await gs.repo.Search<T>(x => x.OwnerId == ownerId, quantity, skip);
                retval.AddRange(newList);
                if (newList.Count < quantity)
                {
                    break;
                }
                skip += quantity;
            }
            return retval;
        }
    }
}
