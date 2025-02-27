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
using Genrpg.Shared.Zones.Settings;
using Genrpg.Shared.Buildings.Settings;

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

        public async Task<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {

            CrawlerMapType mtype = _gameData.Get<CrawlerMapSettings>(_gs.ch).Get(genData.MapTypeId);

            if (mtype == null)
            {
                return null;
            }

            IClientRandom rand = new ClientRandom(world.MaxMapId + 3 + world.Seed / 3);

            if (genData.GenType == null)
            {
                genData.GenType = RandomUtils.GetRandomElement(mtype.GenTypes, rand);
            }

            if (genData.ZoneType == null)
            {
                long zoneTypeId = RandomUtils.GetRandomElement(genData.GenType.WeightedZones, rand).ZoneTypeId;

                genData.ZoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(zoneTypeId);

            }

            if (genData.BuildingArtId == 0)
            {
                genData.BuildingArtId = RandomUtils.GetRandomElement(_gameData.Get<BuildingArtSettings>(_gs.ch).GetData(), rand).IdKey;
            }

            if (genData.ArtSeed == 0)
            {
                genData.ArtSeed = _rand.Next(1000000000); // Use global rand here to make it random each time we generate
            }

            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapTypeId);
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
