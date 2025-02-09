using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Logging.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Client.Assets.Services;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using Genrpg.Shared.Zones.Constants;
using System;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Crawler.Maps.Settings;
using Genrpg.Shared.Crawler.MapGen.Services;
using Genrpg.Shared.Crawler.MapGen.Helpers;
using Genrpg.Shared.Crawler.MapGen.Entities;

namespace Assets.Scripts.Crawler.Maps
{
   

    public class CrawlerMapGenService : ICrawlerMapGenService
    {
        private IAssetService _assetService;
        private ICameraController _cameraController;
        private ICrawlerService _crawlerService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IGameData _gameData;
        private ICrawlerWorldService _worldService;
        private IClientGameState _gs;
        private IClientRandom _rand;

        private CancellationToken _token;

        private PartyData _party;
        private CrawlerWorld _world;

        private SetupDictionaryContainer<long, ICrawlerMapGenHelper> _mapGenHelpers = new SetupDictionaryContainer<long, ICrawlerMapGenHelper>();
        
        public async Task Initialize(CancellationToken token)
        {

            _token = token;

            await Task.CompletedTask;
        }
        public ICrawlerMapGenHelper GetGenHelper(long mapType)
        {
            if (_mapGenHelpers.TryGetValue(mapType, out ICrawlerMapGenHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Awaitable<CrawlerMap> GenerateDungeon(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            List<CrawlerMapType> dungeonTypes = _gameData.Get<CrawlerMapSettings>(_gs.ch).GetData().Where(x=>x.DungeonGenChance > 0).ToList();

            double chanceSum = dungeonTypes.Sum(x => x.DungeonGenChance);

            double chanceChosen = _rand.NextDouble() * chanceSum;

            for (int i = 0; i < dungeonTypes.Count; i++)
            {
                chanceChosen -= dungeonTypes[i].DungeonGenChance;

                if (chanceChosen <= 0)
                {
                    genData.MapType = dungeonTypes[i].IdKey;
                    return await Generate(party, world, genData);
                }
            }
            return null;
        }

        public async Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {

            if (genData.MapType == CrawlerMapTypes.RandomDungeon)
            {

                genData.MapType = CrawlerMapTypes.Dungeon;
                List<CrawlerMapType> dungeonTypes = _gameData.Get<CrawlerMapSettings>(_gs.ch).GetData().Where(x => x.DungeonGenChance > 0).ToList();

                double chanceSum = dungeonTypes.Sum(x => x.DungeonGenChance);

                double chanceChosen = _rand.NextDouble() * chanceSum;

                for (int i = 0; i < dungeonTypes.Count; i++)
                {
                    chanceChosen -= dungeonTypes[i].DungeonGenChance;

                    if (chanceChosen <= 0)
                    {
                        genData.MapType = dungeonTypes[i].IdKey;
                        break;
                    }
                }
            }

            if (genData.ArtSeed == 0)
            {
                genData.ArtSeed = _rand.Next(1000000000);
            }

            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapType);
            NewCrawlerMap newMap = await helper.Generate(party, world, genData);

            if (genData.FromMapId > 0 && newMap.EnterX >= 0 && newMap.EnterZ >= 0)
            {
                LinkTwoMaps(world, genData.FromMapId, genData.FromMapX, genData.FromMapZ, newMap.Map.IdKey, newMap.EnterX, newMap.EnterZ);
            }

            return newMap.Map;
        }

        private void LinkTwoMaps(CrawlerWorld world, long fromMapId, int fromMapX, int fromMapZ, long toMapId, int toMapX, int toMapZ)
        {
            OneWayLink(world, fromMapId, fromMapX, fromMapZ, toMapId, toMapX, toMapZ);
            OneWayLink(world, toMapId, toMapX, toMapZ, fromMapId, fromMapX, fromMapZ);
        }

        private void OneWayLink(CrawlerWorld world, long fromMapId, int fromX, int fromZ, long toMapId, int toX, int toZ)
        {
            CrawlerMap fromMap = world.GetMap(fromMapId);
            CrawlerMap toMap = world.GetMap(toMapId);

            if (fromMap == null || toMap == null)
            {
                return;
            }

            List<MyPoint2> nearbyRoads = new List<MyPoint2>();

            for (int xx = toX -1; xx <= toX + 1; xx++)
            {
                if (xx < 0 || xx > toMap.Width)
                {
                    continue;
                }
                for (int zz = toZ -1; zz <= toZ + 1; zz++)
                {
                    if (zz < 0 || zz >= toMap.Height)
                    {
                        continue;
                    }

                    if (Math.Abs(xx-toX) + Math.Abs(zz-toZ) != 1)
                    {
                        continue;
                    }

                    if (toMap.Get(xx,zz, CellIndex.Terrain) != ZoneTypes.Road)
                    {
                        continue;
                    }
                    nearbyRoads.Add(new MyPoint2(xx, zz));
                }
            }

            if (nearbyRoads.Count > 0)
            {

                long index = fromMapId + toMapId + fromX + fromZ + toX + toZ;
                MyPoint2 chosenRoad = nearbyRoads[(int)(index % nearbyRoads.Count)];

                toX = (int)chosenRoad.X;
                toZ = (int)chosenRoad.Y;
            }

            if (fromX < 0 || fromZ < 0
                || fromX >= fromMap.Width || fromZ >= fromMap.Height ||
                toX < 0 || toZ < 0
                || toX >= toMap.Width || toZ >= toMap.Height)
            {
                return;
            }

            MapCellDetail currentDetail = fromMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == toMapId);

            if (currentDetail == null)
            {
                currentDetail = new MapCellDetail() { EntityTypeId = EntityTypes.Map, EntityId = toMapId };
                fromMap.Details.Add(currentDetail);
            }
            currentDetail.X = fromX;
            currentDetail.Z = fromZ;
            currentDetail.ToX = toX;
            currentDetail.ToZ = toZ;
        }
    }
}
