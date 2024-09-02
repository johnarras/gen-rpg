using Assets.Scripts.Crawler.Maps.Entities;
using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.Crawler.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.Crawler.Tilemaps;
using Assets.Scripts.UI.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Crawler.UI.Screens.Maps
{
    public class CrawlerMapScreen : BaseScreen
    {

        private ICrawlerService _crawlerService;
        private ICrawlerWorldService _worldService;
        private ICrawlerMapService _mapService;
        private IUIService _uiService;
        public CrawlerTilemap Tilemap;
        public GText MapName;

        private PartyData _party;
        private CrawlerWorld _world;
        private CrawlerMap _map;
        protected override async Awaitable OnStartOpen(object data, CancellationToken token)
        {

            if (Tilemap == null)
            {
                StartClose();
                return;
            }
            CrawlerMapScreenArgs mapArgs = data as CrawlerMapScreenArgs;

            _party = _crawlerService.GetParty();

            if (_party == null)
            {
                StartClose();
                return;
            }

            _world = await _worldService.GetWorld(_party.WorldId);

            if (_world == null)
            {
                StartClose();
                return;
            }
                

            if (mapArgs == null)
            {
                mapArgs = new CrawlerMapScreenArgs()
                {
                    MapId = _party.MapId
                };
            }

            _map = _worldService.GetMap(mapArgs.MapId);

            if (_map == null)
            {
                StartClose();
                return;
            }

            _uiService.SetText(MapName,_map.Name);

            CrawlerTilemapInitData initData = new CrawlerTilemapInitData()
            {
                Height = _map.Height,
                Width = _map.Width,
                MapId = _party.MapId,
                XOffset = 0,
                ZOffset = 0,
            };

            await Tilemap.Init(initData);

            await Task.CompletedTask;
        }
    }
}
