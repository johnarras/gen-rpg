using Genrpg.Shared.Client.Core;
using Assets.Scripts.ProcGen.Services;
using Genrpg.Shared.BoardGame.Constants;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.BoardGame.PlayerData;
using Genrpg.Shared.BoardGame.Services;
using Genrpg.Shared.BoardGame.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Genrpg.Shared.Client.Assets.Constants;

namespace Assets.Scripts.ProcGen.Components
{
    public class MarkerPosition
    {
        public int Index { get; set; }
        public float Percent { get; set; }
        public GameObject GameObject { get; set; }
        public Vector3 Position { get; set; }
        public long EntityId { get; set; }
    }

    public class MarkedSpline : BaseBehaviour
    {

        public SplineContainer Container { get; set; }
        private ICurveGenService _curveGenService { get; set; }
        private List<MarkerPosition> _markers = new List<MarkerPosition>();
        public SplineLoopGenParams GenParams { get; set; } = new SplineLoopGenParams();

        private float3 _center = Vector3.zero;

        public float3 Offset { get; set; }

        private int _markerCount = 0;

        public int GetMarkerCount()
        {
            return _markerCount;
        }

        public List<MarkerPosition> GetMarkers()
        {
            return _markers;
        }

        public void Clear()
        {
            if (Container != null)
            {
                foreach (Spline spline in Container.Splines)
                {
                    spline.Clear();
                }
            }

            foreach (MarkerPosition mp in _markers)
            {
                GameObject.Destroy(mp.GameObject);
            }
            _markers.Clear();
        }

        int pipsPerGoldTile = 3;
        int pipsPerOtherTile = 4;
        public void LoadMarkers(IRandom rand, CancellationToken token)
        {
            float length = Container.Splines[0].GetLength();
            _markerCount = (int)(length*GenParams.MarkerDensity);

            BoardData boardData = _gs.ch.Get<BoardData>();

            if (boardData == null || boardData.Length == 0)
            {
                return;
            }

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(_gs.ch).GetData();

            short[] allTiles = boardData.Tiles.Data;

            List<short> mainPath = new List<short>();

            List<List<short>> sidePaths = new List<List<short>>();

            int mainPathLength = -1;
            for (int i = 0; i < allTiles.Length; i++)
            {
                if (allTiles[i] != TileTypes.StartPath)
                {
                    mainPath.Add(allTiles[i]);
                }
                else
                {
                    mainPathLength = i;
                    break;
                }
            }

            _markerCount = mainPathLength;

            List<short> currentPath = null;

            for (int i = mainPathLength; i < allTiles.Length; i++)
            {
                if (currentPath == null)
                {
                    currentPath = new List<short>();    
                    currentPath.Add(allTiles[i]);
                    continue;
                }
                currentPath.Add(allTiles[i]);
                if (allTiles[i] == TileTypes.EndPath)
                {
                    sidePaths.Add(currentPath);
                    currentPath = new List<short>();
                }
            }


            int pipTotal = mainPath.Count * pipsPerGoldTile +
                mainPath.Where(x=>x != TileTypes.Gold).ToList().Count * (pipsPerOtherTile-pipsPerGoldTile)*2;

            int pipsUsed = 0;

            for (int i = 0; i < mainPath.Count; i++)
            {

                TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == mainPath[i]);

                int currPips = tileType.IdKey == TileTypes.Gold ? pipsPerGoldTile : pipsPerOtherTile;

                pipsUsed += currPips-pipsPerGoldTile;

                float percent = 1 - pipsUsed * 1.0f / (pipTotal);

                pipsUsed += currPips;
                Vector3 pos = (Container.Splines[0].EvaluatePosition(percent));
                MarkerPosition markerPos = new MarkerPosition() { Percent = percent, Index = i, Position = pos, EntityId = tileType.IdKey };

                _markers.Add(markerPos);
                _assetService.LoadAssetInto(this, AssetCategoryNames.Tiles, tileType.Art, OnLoadMarker, markerPos, token);
            }

            if (_markers.Count > 0)
            {
                float xcenter = _markers.Sum(m => m.Position.x) / _markers.Count;
                float zcenter = _markers.Sum(m => m.Position.z) / _markers.Count;

                _center = new Vector3(xcenter, 0, zcenter);

                MarkerPosition centerMarker = new MarkerPosition() { Percent = 0, Index = -1, Position = _center, EntityId = 1 };
                _assetService.LoadAssetInto(this, AssetCategoryNames.Tiles, "GoldTile", OnLoadMarker, centerMarker, token);
            }


            if (sidePaths.Count > 0)
            {
                int pathLengthSoFar = mainPath.Count;
                for (int s = 0; s < sidePaths.Count; s++)
                {

                    if (sidePaths[s].Count < 2)
                    {
                        continue;
                    }

                    pipTotal = sidePaths[s].Count * pipsPerGoldTile;
                    pipsUsed = 0;

                    float3 sidePathStart = new Vector3(-50, 0, -50);

                    MarkerPosition pos = _markers.FirstOrDefault(x => x.EntityId == TileTypes.PathEntrance);

                    if (pos != null)
                    {
                        sidePathStart = pos.Position;
                    }

                    Vector3 dirToSidePath = sidePathStart - _center;

                    _curveGenService.CreateLinearSpline(this, s + 1, sidePaths[s].Count, sidePathStart.x, sidePathStart.z, dirToSidePath.x, dirToSidePath.z, rand, token);

                    Spline spline = Container.Splines[s+1];

                    for (int i = 0; i < sidePaths[s].Count; i++)
                    {
                        TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == sidePaths[s][i]);

                        int currPips = pipsPerGoldTile;

                        pipsUsed += currPips - pipsPerGoldTile;

                        float percent = pipsUsed * 1.0f / (pipTotal);

                        pipsUsed += currPips;
                        Vector3 evalPos = (spline.EvaluatePosition(percent));
                        
                        MarkerPosition markerPos = new MarkerPosition() { Percent = percent, Index = i+pathLengthSoFar, Position = evalPos, EntityId = tileType.IdKey };

                        _markers.Add(markerPos);
                        _assetService.LoadAssetInto(this, AssetCategoryNames.Tiles, tileType.Art, OnLoadMarker, markerPos, token);
                    }
                    pathLengthSoFar += sidePaths[s].Count;
                }
            }
        }

        private void OnLoadMarker(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            MarkerPosition markerPos = data as MarkerPosition;
            _markers.Add(markerPos);
            if (go == null)
            {
                Console.WriteLine("MarkerPos: " + markerPos.Index);
                return;
            }

            if (Container == null || Container.Splines.Count < 1)
            {
                _clientEntityService.Destroy(go);
                return;
            }


            markerPos.GameObject = go;

            go.transform.position = markerPos.Position;

            float tileWidth = 1;

            if (go.name.IndexOf("Gold") < 0)
            {
                tileWidth = 1.5f;
            }
            TileType tileType = _gameData.Get<TileTypeSettings>(null).Get(markerPos.EntityId);

            string ttName = "(?)";

            if (tileType !=null)
            {
                ttName = "(" + tileType.Name +")";  
            }

            if (go.transform.childCount > 0)
            {
                go.transform.GetChild(0).localScale = new Vector3(tileWidth, 0.2f, tileWidth);
            }
            go.name = "Tile" + markerPos.Index + " (" + (int)go.transform.position.x + "," + (int)go.transform.position.z + "): " + markerPos.Percent + "% " + ttName;

            if (_markers.Count >= _markerCount)
            {
                _markers = _markers.OrderBy(x => x.Index).ToList();

                for (int m = 0; m  < _markers.Count; m++)
                {
                    MarkerPosition firstPos = _markers[m];
                    MarkerPosition nextPos = _markers[(m+1) % _markers.Count];  

                    if (firstPos.GameObject != null)
                    {
                        var lookAtPos = new Vector3(nextPos.Position.x, firstPos.Position.y, nextPos.Position.z);
                        firstPos.GameObject.transform.LookAt(lookAtPos);
                    }
                }
            }
        }
    }
}
