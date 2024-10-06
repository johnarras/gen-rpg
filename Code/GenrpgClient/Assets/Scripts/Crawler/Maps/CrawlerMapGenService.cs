using Genrpg.Shared.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Maps.Services.GenerateMaps;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Dungeons.Settings;
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

namespace Assets.Scripts.Crawler.Maps
{
   

    public class CrawlerMapGenService : ICrawlerMapGenService
    {
        IAssetService _assetService;
        ICameraController _cameraController;
        ICrawlerService _crawlerService;
        private IDispatcher _dispatcher;
        private ILogService _logService;
        private IGameData _gameData;
        private ICrawlerWorldService _worldService;

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

        public async  Awaitable<CrawlerMap> Generate(PartyData party, CrawlerWorld world, CrawlerMapGenData genData)
        {
            ICrawlerMapGenHelper helper = GetGenHelper(genData.MapType);
            NewCrawlerMap newMap = await helper.Generate(party, world, genData);

            if (genData.FromMapId > 0 && newMap.EnterX >= 0 && newMap.EnterZ >= 0)
            {
                LinkTwoMaps(world, genData.FromMapId, genData.FromMapX, genData.FromMapZ, newMap.Map.IdKey, newMap.EnterX, newMap.EnterZ);
            }

            newMap.Map.DungeonArt = _gameData.Get<DungeonArtSettings>(null).Get(newMap.Map.DungeonArtId);

            return newMap.Map;
        }

        private void LinkTwoMaps(CrawlerWorld world, long mapId1, int mapX1, int mapZ1, long mapId2, int mapX2, int mapZ2)
        {
            OneWayLink(world, mapId1, mapX1, mapZ1, mapId2, mapX2, mapZ2);
            OneWayLink(world, mapId2, mapX2, mapZ2, mapId1, mapX1, mapZ1);
        }

        private void OneWayLink(CrawlerWorld world, long fromMapId, int x, int z, long toMapId, int toX, int toZ)
        {
            CrawlerMap fromMap = world.GetMap(fromMapId);
            CrawlerMap toMap = world.GetMap(toMapId);

            if (fromMap == null || toMap == null
                || x < 0 || z < 0
                || x >= fromMap.Width || z >= fromMap.Height ||
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
            currentDetail.X = x;
            currentDetail.Z = z;
            currentDetail.ToX = toX;
            currentDetail.ToZ = toZ;
        }
    }
}
