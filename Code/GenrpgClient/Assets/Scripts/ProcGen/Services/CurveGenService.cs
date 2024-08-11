using Genrpg.Shared.Utils.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine.Splines;
using UnityEngine;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.Utils;
using Assets.Scripts.ProcGen.Components;
using System.Security.Policy;
using Genrpg.Shared.Logging.Interfaces;
using Unity.Properties;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.BoardGame.Services;
using Genrpg.Shared.GameSettings;

namespace Assets.Scripts.ProcGen.Services
{
    public interface ICurveGenService : IInjectable
    {
        MarkedSpline CreateSpline(MarkedSpline spline, IRandom rand, CancellationToken token);
    }

    public class SplineLoopGenParams
    {
        public int MinKnots = 7;
        public int MaxKnots = 10;
        public int MinRadius = 50;
        public int MaxRadius = 100;
        public float MarkerDensity = 0.1f;
        public Vector3 Position = Vector3.zero;
        public float StartAngle = 0;
        public MarkedSpline MarkedSpline = null;
        public float BreakChance = 0;
    }

    public class CurveGenService : ICurveGenService
    {
        private ILogService _logService;
        private IAssetService _assetService;
        private IGameObjectService _gameObjectService;
        private ISharedBoardGenService _boardGenService;
        private IGameData _gameData;

        public MarkedSpline CreateSpline(MarkedSpline markedSpline, IRandom rand, CancellationToken token)
        {

            markedSpline.Container = _gameObjectService.GetOrAddComponent<SplineContainer>(markedSpline.gameObject);

            if (markedSpline.Container.Splines.Count < 1)
            {
                markedSpline.Container.AddSpline();
            }

            markedSpline.Clear();

            TangentMode tmode = TangentMode.Continuous;
            Spline spline = markedSpline.Container.Splines[0];

            int knotCount = MathUtils.IntRange(markedSpline.GenParams.MinKnots, markedSpline.GenParams.MaxKnots, rand);
            float angleGap = 360 / knotCount;
            float angleDelta = 0.9f;

            float minRad = markedSpline.GenParams.MinRadius;
            float maxRad = markedSpline.GenParams.MaxRadius;

            markedSpline.GenParams.StartAngle = MathUtils.FloatRange(0, Mathf.PI * 2, rand);

            float xrad = MathUtils.FloatRange(minRad, maxRad, rand);
            float zrad = MathUtils.FloatRange(minRad, maxRad, rand);
            float radDelta = 0.6f;

            spline.Closed = true;

            float angle = 0;

            float radPct = MathUtils.FloatRange(1 - radDelta, 1 + radDelta, rand);

            while (angle < 360 - angleGap)
            {
                float currAngle = angle + markedSpline.GenParams.StartAngle;

                while (currAngle >= 360)
                {
                    currAngle -= 360;
                }
                while (currAngle < 0)
                {
                    currAngle += 360;
                }

                float x = Mathf.Cos(currAngle * Mathf.PI / 180) * xrad * radPct;
                float z = Mathf.Sin(currAngle * Mathf.PI / 180) * xrad * radPct;

                TangentMode knotTmode = (rand.NextDouble() < markedSpline.GenParams.BreakChance ? TangentMode.Broken : tmode);

                spline.Add(new Vector3(x+markedSpline.GenParams.Position.x, markedSpline.GenParams.Position.y, z+markedSpline.GenParams.Position.z), knotTmode);

                float oldRadPct = radPct;
                radPct = MathUtils.FloatRange(1 - radDelta, 1 + radDelta, rand);

                if (radPct < 0.5f)
                {
                    radPct *= 1.1f;
                }
                if (radPct > 1.5f)
                {
                    radPct /= 1.1f;
                }

                angle += angleGap * MathUtils.FloatRange((1 - angleDelta), (1 + angleDelta), rand);
            }


            markedSpline.LoadMarkers(rand, token);
            return markedSpline;
        }
    }
}
