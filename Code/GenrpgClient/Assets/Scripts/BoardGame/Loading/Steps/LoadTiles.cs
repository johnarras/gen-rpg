using Assets.Scripts.BoardGame.Controllers;
using Assets.Scripts.BoardGame.Loading.Constants;
using Assets.Scripts.BoardGame.Tiles;
using Assets.Scripts.GameObjects;
using Assets.Scripts.ProcGen.Components;
using Assets.Scripts.ProcGen.Services;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.Client.Assets.Constants;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Tiles.Settings;
using Genrpg.Shared.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace Assets.Scripts.BoardGame.Loading
{
    public class LoadTiles : BaseLoadBoardStep
    {
        protected ICurveGenService _curveGenService;
        protected IMapTerrainManager _terrainManager;
        private IMapGenData _mapGenData;
        private IBoardGameController _boardGameController;
        private IGameData _gameData;
        private IClientGameState _gs;
        public override ELoadBoardSteps GetKey() { return ELoadBoardSteps.LoadTiles; }

        public override async Awaitable Execute(BoardData boardData, CancellationToken token)
        {
            GameObject parent = _controller.GetBoardAnchor();
            MarkedSpline _spline;
            IRandom _rand = new MyRandom(boardData.Seed);

            int radius = 20;
            _spline = _clientEntityService.GetOrAddComponent<MarkedSpline>(parent);
            _spline.GenParams.BreakChance = MathUtils.FloatRange(0.1f, 0.7f, _rand);
            _spline.GenParams.MinRadius = radius;
            _spline.GenParams.MaxRadius = _spline.GenParams.MinRadius * 5 / 4;

            float offset = MapConstants.TerrainPatchSize;
            _spline.GenParams.Position = new Vector3(offset, 0, offset);
            _curveGenService.CreateCircularSpline(_spline, _rand, token);

            List<MarkerPosition> markers = _spline.GetMarkers();

            while (markers.Any(x=>x.GameObject == null))
            {
                await Awaitable.NextFrameAsync(token);
            }

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(_gs.ch).GetData();

            float doffset = 0.5f;

            List<TileController> controllers = new List<TileController>();
            foreach (MarkerPosition markerPos in _spline.GetMarkers())
            {
                if (markerPos.GameObject != null)
                {
                    Vector3 pos = markerPos.GameObject.transform.position;

                    float maxHeight = -1000;

                    for (int xx = -1; xx <= 1; xx++)
                    {
                        for (int zz = -1; zz <= 1; zz++)
                        {
                            float x = pos.x + xx * doffset;
                            float z = pos.z + zz * doffset;


                            float height = _terrainManager.GetInterpolatedHeight(x,z);

                            if (height > maxHeight)
                            {
                                maxHeight = height; 
                            }
                        }
                    }

                    if (markerPos.Index < 0)
                    {
                        continue;
                    }
                    long tileTypeId = boardData.Tiles[markerPos.Index];

                    TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == tileTypeId);

                    TileTypeWithIndex ttidx = new TileTypeWithIndex()
                    {
                        TileType = tileType,
                        Index = markerPos.Index,
                    };

                    TileController tileArt = await _assetService.CreateAsync<TileController,TileTypeWithIndex>(ttidx, AssetCategoryNames.Tiles, tileType.Art, markerPos.GameObject,
                        token);
                    controllers.Add(tileArt);
                    tileArt.MarkerPos = markerPos;
                    markerPos.GameObject.transform.position = new Vector3(pos.x, maxHeight+0.3f, pos.z);

                }
            }

            _boardGameController.SetTiles(controllers);
        }
    }
}
