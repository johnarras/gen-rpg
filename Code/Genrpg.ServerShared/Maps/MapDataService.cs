using Genrpg.ServerShared.Core;
using Genrpg.ServerShared.Maps.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Indexes;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
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
    public interface IMapDataService : IInitializable
    {
        Task<List<MapStub>> GetMapStubs();
        Task<Map> LoadMap(IRandom rand, string mapId);
        Task SaveMap(IRepositoryService repoService, Map map);
    }

    public class MapDataService : IMapDataService
    {
        protected IRepositoryService _repoService = null;
        protected ILogService _logService = null;
        public async Task Initialize( CancellationToken token)
        {
            CreateIndexData data = new CreateIndexData();
            data.Configs.Add(new IndexConfig() { MemberName = nameof(QuestType.OwnerId) });
            data.Configs.Add(new IndexConfig() { MemberName =  nameof(QuestType.MapId) });
            List<Task> tasks = new List<Task>();
            tasks.Add(_repoService.CreateIndex<QuestType>(data));
            tasks.Add(_repoService.CreateIndex<QuestItem>(data));
            tasks.Add(_repoService.CreateIndex<Zone>(data));
            await Task.WhenAll(tasks);
            await Task.CompletedTask;
        }

        public async Task<List<MapStub>> GetMapStubs()
        {
            List<MapRoot> allMaps = await _repoService.Search<MapRoot>(x => true);

            List<MapStub> allStubs = new List<MapStub>();

            foreach (MapRoot map in allMaps)
            {
                MapStub stub = new MapStub();
                stub.CopyFrom(map);
                allStubs.Add(stub);
            }
            return allStubs;
        }

        public async Task<Map> LoadMap(IRandom rand, string mapId)
        {
            MapRoot root = await _repoService.Load<MapRoot>(mapId);

            if (root == null)
            {
                root = new MapRoot()
                {
                    Id = mapId,
                    MapVersion = 0
                };
                await _repoService.Save(root);
            }

            Map map = SerializationUtils.Deserialize<Map>(SerializationUtils.Serialize(root));

            string mapOwnerId = Map.GetMapOwnerId(map);

            map.Zones = await LoadMapDataList<Zone>(rand, mapOwnerId);
            map.Quests = await LoadMapDataList<QuestType>(rand, mapOwnerId);
            map.QuestItems = await LoadMapDataList<QuestItem>(rand, mapOwnerId);

            return map;
        }

        public async Task SaveMap(IRepositoryService repoService, Map map)
        {

            try
            {
                MapRoot mapRoot = SerializationUtils.Deserialize<MapRoot>(SerializationUtils.Serialize(map));

                // Do not save map. It's too big.
                await _repoService.Save(mapRoot);

                string mapOwnerId = Map.GetMapOwnerId(map);

                await SaveMapDataList(repoService, map.Zones, map.Id, map.MapVersion);
                await SaveMapDataList(repoService, map.Quests, map.Id, map.MapVersion);
                await SaveMapDataList(repoService, map.QuestItems, map.Id, map.MapVersion);
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "SaveMap");
            }
        }

        protected async Task SaveMapDataList<T>(IRepositoryService repoService, List<T> list, string mapId, int mapVersion) where T : class, IMapOwnerId, IId
        {
            await repoService.DeleteAll<T>(x => x.MapId == mapId);
            string ownerId = Map.GetMapOwnerId(mapId, mapVersion);
            foreach (T t in list)
            {
                t.OwnerId = ownerId;
                t.MapId = mapId;
                t.Id = t.IdKey + "-" + ownerId;
            }
            await repoService.SaveAll(list);
        }

        protected async Task<List<T>> LoadMapDataList<T>(IRandom rand, string ownerId) where T : class, IStringOwnerId
        {
            List<T> retval = new List<T>();

            int quantity = 10000;
            int skip = 0;

            while (true)
            {
                List<T> newList = await _repoService.Search<T>(x => x.OwnerId == ownerId, quantity, skip);
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
