using Assets.Scripts.ProcGen.Services;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public void LoadMarkers(IRandom rand, CancellationToken token)
        {
            float length = Container.Splines[0].GetLength();
            _markerCount = (int)(length*GenParams.MarkerDensity);
            if (GenParams.MarkerQuantity > 0)
            {
                _markerCount = GenParams.MarkerQuantity;
            }

            _logService.Info("LENGTH: " + length);

            for (int i = 0; i < _markerCount; i++)
            {
                float percent = i * 1.0f / _markerCount;
                Vector3 pos = Container.Splines[0].EvaluatePosition(percent);
                pos += new Vector3(0, i, 0);
                MarkerPosition markerPos = new MarkerPosition() { Percent = percent, Index = i, Position = pos };
                _assetService.LoadAssetInto(this, AssetCategoryNames.Buildings, "Default/House1", OnLoadMarker, markerPos, token);
            }
        }

        private void OnLoadMarker(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (Container == null || Container.Splines.Count < 1)
            {
                GEntityUtils.Destroy(go);
                return;
            }

            MarkerPosition markerPos = data as MarkerPosition;
            _markers.Add(markerPos);

            markerPos.GameObject = go;

            go.transform.position = markerPos.Position;

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
