using System.Linq;
using System.Threading;
using UnityEngine.Splines;
using UnityEngine;
using Genrpg.Shared.Utils;
using Assets.Scripts.ProcGen.Components;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.BoardGame.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Client.Assets;
using Genrpg.Shared.Client.Assets.Services;

namespace Assets.Scripts.ProcGen.Services
{
    public interface ICurveGenService : IInjectable
    {
        MarkedSpline CreateCircularSpline(MarkedSpline markedSpline, IRandom rand, CancellationToken token);
        MarkedSpline CreateLinearSpline(MarkedSpline markedSpline, int splineIndex, int pathLength, float sx, float sy, float dx, float dz, IRandom rand, CancellationToken token);
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
        private IClientEntityService _clientEntityService;
        private ISharedBoardGenService _boardGenService;
        private IGameData _gameData;


        public MarkedSpline CreateLinearSpline(MarkedSpline markedSpline, int splineIndex, int pathLength, float sx, float sz, float dx, float dz, IRandom rand, CancellationToken token)
        {

            markedSpline.Container = _clientEntityService.GetOrAddComponent<SplineContainer>(markedSpline.gameObject);

            while (markedSpline.Container.Splines.Count <= splineIndex)
            {
                markedSpline.Container.AddSpline();
            }

            Spline spline = markedSpline.Container.Splines.Last();

            spline.Closed = false;

            float x = 0;
            float z = 0;

            float totalDist = Mathf.Sqrt(dx * dx + dz * dz);

            dx /= totalDist;
            dz /= totalDist;

            int startDist = 3;

            x = sx + dx * startDist;
            z = sz + dz * startDist;

            float distDelta = 0.5f;

            int segmentLength = 2;

            spline.Add(new Vector3(x, 0, z), TangentMode.Continuous);
            while (true)
            {
               float newDist = MathUtils.IntRange(segmentLength/2, segmentLength*3/2, rand);

                x += dx * newDist * MathUtils.FloatRange(1 - distDelta, 1 + distDelta, rand);
                z += dz * newDist * MathUtils.FloatRange(1 - distDelta, 1 + distDelta, rand);

                float deltaLength = newDist * 2 / 3;

                x += MathUtils.FloatRange(-deltaLength, deltaLength, rand);
                z += MathUtils.FloatRange(-deltaLength, deltaLength, rand);

                TangentMode tmode = (rand.NextDouble() < markedSpline.GenParams.BreakChance ? TangentMode.Broken : TangentMode.Continuous);
                spline.Add(new Vector3(x, 0, z), tmode);

                if (spline.GetLength() >= 2.3*pathLength)
                {
                    break;
                }
            }

            return markedSpline;
        }

        public MarkedSpline CreateCircularSpline(MarkedSpline markedSpline, IRandom rand, CancellationToken token)
        {

            markedSpline.Container = _clientEntityService.GetOrAddComponent<SplineContainer>(markedSpline.gameObject);

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

            markedSpline.GenParams.StartAngle = MathUtils.FloatRange(0, 360, rand);

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

                TangentMode knotMode = (rand.NextDouble() < markedSpline.GenParams.BreakChance ? TangentMode.Broken : tmode);

                spline.Add(new Vector3(x+markedSpline.GenParams.Position.x, markedSpline.GenParams.Position.y, z+markedSpline.GenParams.Position.z), knotMode);

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
