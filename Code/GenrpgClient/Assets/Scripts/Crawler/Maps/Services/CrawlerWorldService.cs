using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Model;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Loot.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.TimeOfDay.Settings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public class CrawlerWorldService : ICrawlerWorldService
    {
        private IRepositoryService _repoService;
        private IGameData _gameData;
        private ICrawlerMapService _mapService;
        private ICrawlerMapGenService _mapGenService;
        private ICrawlerService _crawlerService;
        private ILogService _logService;
        private ILootGenService _lootGenService;
        private IClientRandom _rand;
        private IDispatcher _dispatcher;
        private IClientAppService _clientAppService;
        private IClientGameState _gs;

        private CrawlerWorld _world = null;

        public async Task<CrawlerWorld> GenerateWorld(PartyData partyData)
        {

            partyData.GameMode = _gs.GameMode;
            partyData.Reset(_rand);
            if (partyData.Members.Count < 1)
            {
                partyData.MaxLevel = 0;
            }
            CrawlerWorld world = await GenerateInternal(partyData.WorldId);
            _dispatcher.Dispatch(new CrawlerUIUpdate());
            return world;
        }

        public CrawlerMap GetMap(long mapId)
        {
            return _world?.GetMap(mapId) ?? null;
        }

        public async Task<CrawlerWorld> GetWorld(long worldId)
        {

            if (_world != null && _world.IdKey == worldId)
            {
                return _world;
            }

            CrawlerWorld world = null;

            try
            {
                world = await LoadWorld(worldId);
            }
            catch (Exception ex)
            {
                _logService.Warning("Bad map load: " + ex.Message);
            }

            if (world == null || world.IdKey != worldId)
            {
                world = await GenerateInternal(worldId);
            }

            _world = world;
            return world;
        }

        private string WorldPathPrefix(long worldId)
        {
            return (Application.isEditor?"Editor":"") + "World" + "/";
        }

        public async Task SaveWorld(CrawlerWorld world)
        {
            ClientRepositoryService clientRepoService = _repoService as ClientRepositoryService;

            string pathPrefix = WorldPathPrefix(world.IdKey);

            List<Task> allTasks = new List<Task>();

            await clientRepoService.StringSave<CrawlerWorld>(pathPrefix + world.Id, SerializationUtils.Serialize(world));

            foreach (CrawlerMap map in world.Maps)
            {
                await SaveMap(world, map);
            }

        }

        public async Task SaveMap(CrawlerWorld world, CrawlerMap map)
        {
            ClientRepositoryService clientRepoService = _repoService as ClientRepositoryService;
            string pathPrefix = WorldPathPrefix(world.IdKey);
            map.Data = CompressionUtils.CompressBytes(map.Data);
            await clientRepoService.StringSave<CrawlerMap>(pathPrefix + map.Id, SerializationUtils.Serialize(map));
            map.Data = CompressionUtils.DecompressBytes(map.Data);
        }

        private async Task<CrawlerWorld> LoadWorld(long worldId)
        {

            ClientRepositoryService clientRepoService = _repoService as ClientRepositoryService;

            string pathPrefix = WorldPathPrefix(worldId);

            CrawlerWorld world = await clientRepoService.LoadObjectFromString<CrawlerWorld>(pathPrefix + "World");

            if (world == null)
            {
                return null;
            }

            world.Maps = new List<CrawlerMap>();

            List<Task<CrawlerMap>> loadTasks = new List<Task<CrawlerMap>>();

            for (int i = 1; i <= world.MaxMapId; i++)
            {
                loadTasks.Add(clientRepoService.LoadObjectFromString<CrawlerMap>(pathPrefix + "Map" + i));
            }

            CrawlerMap[] allMaps = await Task.WhenAll(loadTasks);

            world.Maps.AddRange(allMaps.Where(x => x != null).OrderBy(x => x.IdKey));

            foreach (CrawlerMap map in world.Maps)
            {
                map.Data = CompressionUtils.DecompressBytes(map.Data);
            }

            return world;
        }

        private async Task<CrawlerWorld> GenerateInternal(long worldId)
        {

            PartyData partyData = _crawlerService.GetParty();

            if (partyData.GameMode == EGameModes.Roguelike)
            {
                try
                {
                    CrawlerWorld world = new CrawlerWorld() { Id = "World", Name = "World", IdKey = worldId };

                    ICrawlerMapGenHelper helper = _mapGenService.GetGenHelper(CrawlerMapTypes.City);

                    MyRandom rand = new MyRandom(worldId + 1);

                    CrawlerMapGenData genData = new CrawlerMapGenData()
                    {
                        MapType = CrawlerMapTypes.City,
                        World = world,
                        Level = 1,
                        Looping = false,
                        ZoneTypeId = 0,
                    };

                    CrawlerMap outdoorMap = await _mapGenService.Generate(_crawlerService.GetParty(), world, genData);

                    string path = _clientAppService.PersistentDataPath + "/Data/World";

#if UNITY_EDITOR
                    path = _clientAppService.PersistentDataPath + "/Data/EditorWorld";
#endif
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    await SaveWorld(world);

                    await _crawlerService.SaveGame();
                    _crawlerService.ClearAllStates();
                    _mapService.CleanMap();



                    StringBuilder riddleSB = new StringBuilder();
                    foreach (CrawlerMap map in world.Maps)
                    {
                        if (!string.IsNullOrEmpty(map.RiddleText) && !string.IsNullOrEmpty(map.RiddleAnswer))
                        {
                            riddleSB.Clear();

                            riddleSB.Append(map.Name + " [#" + map.IdKey + "] ");
                            riddleSB.Append("Riddle Answer: " + map.RiddleAnswer + "\n");
                            riddleSB.Append(map.RiddleText);
                            _logService.Info(riddleSB.ToString());
                        }
                    }


                    _dispatcher.Dispatch(new ClearCrawlerTilemaps());

                    return world;
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "CrawlerWorldGen");
                }
                return null;
            }
            else
            {
                try
                {
                    CrawlerWorld world = new CrawlerWorld() { Id = "World", Name = "World", IdKey = worldId };

                    ICrawlerMapGenHelper helper = _mapGenService.GetGenHelper(CrawlerMapTypes.Outdoors);

                    MyRandom rand = new MyRandom(worldId + 1);

                    CrawlerMapGenData genData = new CrawlerMapGenData()
                    {
                        MapType = CrawlerMapTypes.Outdoors,
                        World = world,
                        Level = 1,
                        Looping = false,
                        ZoneTypeId = 0,
                    };

                    CrawlerMap outdoorMap = await _mapGenService.Generate(_crawlerService.GetParty(), world, genData);

                    string path = _clientAppService.PersistentDataPath + "/Data/World";
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                    await SaveWorld(world);

                    await _crawlerService.SaveGame();
                    _crawlerService.ClearAllStates();
                    _mapService.CleanMap();



                    StringBuilder riddleSB = new StringBuilder();
                    foreach (CrawlerMap map in world.Maps)
                    {
                        if (!string.IsNullOrEmpty(map.RiddleText) && !string.IsNullOrEmpty(map.RiddleAnswer))
                        {
                            riddleSB.Clear();

                            riddleSB.Append(map.Name + " [#" + map.IdKey + "] ");
                            riddleSB.Append("Riddle Answer: " + map.RiddleAnswer + "\n");
                            riddleSB.Append(map.RiddleText);
                            _logService.Info(riddleSB.ToString());
                        }
                    }


                    _dispatcher.Dispatch(new ClearCrawlerTilemaps());

                    return world;
                }
                catch (Exception e)
                {
                    _logService.Exception(e, "CrawlerWorldGen");
                }
                return null;
            }
        }


        public async Task<ZoneType> GetCurrentZone(PartyData partyData)
        {

            CrawlerWorld world = await GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(partyData.MapId);

            if (map == null)
            {
                return null;
            }

            CrawlerMapType mapType = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(map.CrawlerMapTypeId);

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(_gs.ch).GetData();

            if (mapType.ZoneTypeId > 0)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey == mapType.ZoneTypeId);
            }

            int index = map.GetIndex(partyData.MapX, partyData.MapZ);

            if (map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey > 0);
            }

            ZoneType ztype = allZoneTypes.FirstOrDefault(x=>x.IdKey == map.Get(partyData.MapX, partyData.MapZ, CellIndex.Terrain));

            if (ztype == null || ztype.ZoneUnitSpawns.Count < 1)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey > 0);
            }

            return ztype;

        }

        public async Task<long> GetMapLevelAtParty(CrawlerWorld world, PartyData party)
        {
            return await GetMapLevelAtPoint(world, party.MapId, party.MapX, party.MapZ);
        }

        public async Task<long> GetMapLevelAtPoint(CrawlerWorld world, long mapId, int x, int z)
        {
            CrawlerMap map = world.GetMap(mapId);
            
            if (map == null)
            {
                return 1;
            }

            if (map.CrawlerMapTypeId != CrawlerMapTypes.Outdoors)
            {
                return map.Level;
            }

            List<MapCellDetail> startDetails = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<MapCellDetail> finalDetails = new List<MapCellDetail>();

            foreach (MapCellDetail detail in startDetails)
            {
                CrawlerMap cityMap = world.GetMap(detail.EntityId);

                if (cityMap != null && cityMap.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    finalDetails.Add(detail);
                }
            }
            
            Dictionary<MapCellDetail, long> cityDistances = new Dictionary<MapCellDetail, long>();

            double px = x;
            double pz = z;

            foreach (MapCellDetail detail in finalDetails)
            {
                double distance = Math.Sqrt((px - detail.X) * (px - detail.X) + (pz - detail.Z) * (pz - detail.Z));

                cityDistances[detail] = (long)distance;  
            }

            List<long> orderedDistances = cityDistances.Values.OrderBy(x => x).ToList();

            MapCellDetail firstDetail = null;
            long firstDist = 0;
            long firstLevel = 0;
            MapCellDetail secondDetail = null;
            long secondDist = 0;
            long secondLevel = 0;

            firstDist = orderedDistances[0];
            secondDist = orderedDistances[1];

            foreach (MapCellDetail detail in cityDistances.Keys)
            {
                if (cityDistances[detail] == firstDist)
                {
                    firstDetail = detail; 
                    CrawlerMap cityMap = world.GetMap(detail.EntityId);
                    firstLevel = cityMap.Level;
                }
                if (cityDistances[detail] == secondDist)
                {
                    secondDetail = detail;
                    CrawlerMap cityMap = world.GetMap(detail.EntityId);
                    secondLevel = cityMap.Level;
                }
            }


            if (firstDist <= 0 && secondDist <= 0)
            {
                return firstLevel;
            }

            double distanceSum = firstDist + secondDist;

            double firstDistPct = firstDist / distanceSum;
            double secondDistPct = secondDist / distanceSum;

            await Task.CompletedTask;
            return (long)(firstDistPct * firstLevel + secondDistPct * secondLevel);
        }
    }
}
