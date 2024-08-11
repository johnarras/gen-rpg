using Assets.Scripts.ProcGen.RandomNumbers;
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

namespace Assets.Scripts.ProcGen.Components
{
    public class MarkerPosition
    {
        public int Index { get; set; }
        public float Percent { get; set; }
        public GameObject GameObject { get; set; }
        public Vector3 Position { get; set; }
    }

    public class MarkedSpline : BaseBehaviour
    {

        public SplineContainer Container { get; set; }

        private List<MarkerPosition> _markers = new List<MarkerPosition>();
        public SplineLoopGenParams GenParams { get; set; } = new SplineLoopGenParams();


        public float3 Offset { get; set; }

        private int _markerCount = 0;

        public int GetMarkerCount()
        {
            return _markerCount;
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

            float desiredLength = 8 * boardData.Length;

            float lengthRatio = desiredLength/Container.Splines[0].GetLength();

            IReadOnlyList<TileType> tileTypes = _gameData.Get<TileTypeSettings>(_gs.ch).GetData();

            short[] tiles = boardData.Tiles.Data;

            int pipTotal = boardData.Length * pipsPerGoldTile +
                tiles.Where(x=>x != TileTypes.Gold).ToList().Count * (pipsPerOtherTile-pipsPerGoldTile)*2;

            int pipsUsed = 0;

            for (int i = 0; i < tiles.Length; i++)
            {

                TileType tileType = tileTypes.FirstOrDefault(x => x.IdKey == tiles[i]);

                int currPips = tileType.IdKey == TileTypes.Gold ? pipsPerGoldTile : pipsPerOtherTile;

                pipsUsed += currPips-pipsPerGoldTile;

                float percent = pipsUsed * 1.0f / (pipTotal);

                pipsUsed += currPips;
                Vector3 pos = (Container.Splines[0].EvaluatePosition(percent) + Offset);
                pos *= lengthRatio;
                MarkerPosition markerPos = new MarkerPosition() { Percent = percent, Index = i, Position = pos };

                _assetService.LoadAssetInto(this, AssetCategoryNames.Tiles, tileType.Art, OnLoadMarker, markerPos, token);
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
                GEntityUtils.Destroy(go);
                return;
            }


            markerPos.GameObject = go;

            go.transform.position = markerPos.Position;

            float tileWidth = 5;

            if (go.name.IndexOf("Gold") < 0)
            {
                tileWidth = 6.5f;
            }
            go.transform.localScale = new Vector3(tileWidth, 0.2f, tileWidth);
            go.name = "Tile" + markerPos.Index + " (" + (int)go.transform.position.x + "," + (int)go.transform.position.z + "): " + markerPos.Percent + "%";

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
