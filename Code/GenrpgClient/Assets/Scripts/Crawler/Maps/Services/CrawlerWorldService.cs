using Assets.Scripts.Crawler.Maps.Constants;
using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Zones.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.Services
{
    public class CrawlerWorldService : ICrawlerWorldService
    {

        private IRepositoryService _repoService;
        private IGameData _gameData;
        private ICrawlerMapService _mapService;

        string WorldMapFilePrefix = "CrawlerWorld";

        private CrawlerWorld _world = null;

        private bool _didLoad = false;
        public async Awaitable<CrawlerWorld> GetWorld(long worldId)
        {

            if (_world != null && _world.IdKey == worldId && _didLoad)
            {
                return _world;
            }

            CrawlerWorld world = await _repoService.Load<CrawlerWorld>(WorldMapFilePrefix + worldId);    

            if (world == null || !_didLoad)
            {
                world = Generate(worldId);

                await SaveWorld(world);
            }

            _didLoad = true;
            _world = world;
            return world;
        }

        public void SetWorld(CrawlerWorld world)
        {
            _world = world; 
        }

        public async Awaitable SaveWorld(CrawlerWorld world)
        {
            await _repoService.Save(world);
        }

        public CrawlerWorld Generate(long worldId)
        {
            CrawlerWorld world = new CrawlerWorld() { Id = WorldMapFilePrefix + worldId, Name = "World" + worldId, IdKey = worldId };

            ICrawlerMapTypeHelper helper = _mapService.GetHelper(Constants.ECrawlerMapTypes.Outdoors);

            CrawlerMapGenData mgd = new CrawlerMapGenData()
            {
                MapType = Constants.ECrawlerMapTypes.Outdoors,
                World = world,
                Level = 1,
                Width = 96,
                Height = 64,
                Looping = false,
                ZoneTypeId = 0,
            };

            CrawlerMap outdoorMap = helper.Generate(mgd);


            return world;
        }


        public async Awaitable<ZoneType> GetCurrentZone(PartyData partyData)
        {

            CrawlerWorld world = await GetWorld(partyData.WorldId);

            CrawlerMap map = world.GetMap(partyData.MapId);

            IReadOnlyList<ZoneType> allZoneTypes = _gameData.Get<ZoneTypeSettings>(null).GetData();

            if (map.ZoneTypeId > 0)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey == map.ZoneTypeId);
            }


            int index = map.GetIndex(partyData.MapX, partyData.MapZ);

            if (map.MapType != ECrawlerMapTypes.Outdoors || index < 0 || index >= map.CoreData.Length)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey > 0);
            }

            ZoneType ztype = allZoneTypes.FirstOrDefault(x=>x.IdKey == map.CoreData[map.GetIndex(partyData.MapX,partyData.MapZ)]);

            if (ztype == null || ztype.ZoneUnitSpawns.Count < 1)
            {
                return allZoneTypes.FirstOrDefault(x => x.IdKey > 0);
            }

            return ztype;

        }


        public async Awaitable<long> GetMapLevelAtParty(PartyData partyData)
        {
            CrawlerMap map = _world.GetMap(partyData.MapId);
            
            if (map.MapType != ECrawlerMapTypes.Outdoors)
            {
                return map.Level;
            }

            List<MapCellDetail> startDetails = map.Details.Where(x => x.EntityTypeId == EntityTypes.Map).ToList();

            List<MapCellDetail> finalDetails = new List<MapCellDetail>();

            foreach (MapCellDetail detail in startDetails)
            {
                CrawlerMap cityMap = _world.GetMap(detail.EntityId);

                if (cityMap != null && cityMap.MapType == ECrawlerMapTypes.City)
                {
                    finalDetails.Add(detail);
                }
            }
            
            Dictionary<MapCellDetail, long> cityDistances = new Dictionary<MapCellDetail, long>();

            double px = partyData.MapX;
            double pz = partyData.MapZ;

            foreach (MapCellDetail detail in finalDetails)
            {
                double distance = (px - detail.X) * (px - detail.X) + (pz - detail.Z) * (pz - detail.Z);

                cityDistances[detail] = (long)distance;  
            }

            List<long> orderedDistances = cityDistances.Values.OrderBy(x => x).ToList();

            MapCellDetail firstDetail = null;
            long firstDist = 0;
            MapCellDetail secondDetail = null;
            long secondDist = 0;

            firstDist = orderedDistances[0];
            secondDist = orderedDistances[1];

            foreach (MapCellDetail detail in cityDistances.Keys)
            {
                if (cityDistances[detail] == firstDist)
                {
                    firstDetail = detail;
                }
                if (cityDistances[detail] == secondDist)
                {
                    secondDetail = detail;
                }
            }


            if (firstDist <= 0 && secondDist <= 0)
            {
                return firstDetail.Value;
            }

            double distanceSum = firstDist + secondDist;

            double firstDistPct = firstDist / distanceSum;
            double secondDistPct = secondDist / distanceSum;

            return (long)(firstDistPct * firstDetail.Value + secondDistPct * secondDetail.Value);
        }
    }
}
