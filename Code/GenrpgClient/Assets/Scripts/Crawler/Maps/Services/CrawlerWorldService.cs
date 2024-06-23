using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services.Helpers;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Biomes.Settings;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //_didLoad = true;
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

            IReadOnlyList<BiomeType> biomes = _gameData.Get<BiomeSettings>(null).GetData();

            ICrawlerMapTypeHelper helper = _mapService.GetHelper(Constants.ECrawlerMapTypes.Outdoors);

            CrawlerMapGenData mgd = new CrawlerMapGenData()
            {
                MapType = Constants.ECrawlerMapTypes.Outdoors,
                World = world,
            };

            CrawlerMap outdoorMap = helper.Generate(mgd);


            return world;
        }
    }
}
