using MessagePack;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using Genrpg.Shared.ProcGen.Settings.LineGen;

namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// List of geometric utils.
    /// </summary>
    [MessagePackObject]
    public class GeomUtils
    {
        public const float Epsilon = 0.0001f;

        public const float BadDistance = -1;


        public const float MaxDistance = 100000000;

        /// <summary>
        /// Distance from point to actual segment
        /// </summary>
        /// <param name="ls">Line segment</param>
        /// <param name="px">x val of point</param>
        /// <param name="py">y val of point</param>
        /// <returns>distance or -1 if failure</returns>
        public static float DistanceFromPointToLineSegment(LineSegment ls, float px, float py)
        {
            if (ls == null)
            {
                return BadDistance;
            }

            return DistanceFromPointToLineSegment(ls.SX, ls.SY, ls.EX, ls.EY, px, py);
        }

        /// <summary>
        /// Get the distnace from point (px,py) to line segment (ex,ey),(sx,sy)
        /// </summary>
        /// <param name="sx">Start x</param>
        /// <param name="sy">Start y</param>
        /// <param name="ex">End x</param>
        /// <param name="ey">End y</param>
        /// <param name="px">Point x off line</param>
        /// <param name="pz">Point y off line</param>
        /// <returns>distance</returns>
        /// 
        public static float DistanceFromPointToLineSegment(float sx, float sy, float ex, float ey, float px, float pz)
        {
            float ly = ey - sy;
            float lx = ex - sx;

            float segmentLength = lx * lx + ly * ly;
            if (Math.Sqrt(segmentLength) < Epsilon)
            {
                return (float)Math.Sqrt((px - ex) * (px - ex) + (pz - ey) * (pz - ey));
            }

            float u = ((px - sx) * lx + (pz - sy) * ly) / segmentLength;

            u = MathUtils.Clamp(0, u, 1);

            float x = sx + u * lx;
            float z = sy + u * ly;

            float dx = x - px;
            float dz = z - pz;

            return (float)Math.Sqrt(dx * dx + dz * dz);

        }

        public static float DistanceFromPointToPolyLine<T>(List<T> list, float px, float py) where T : LineSegment
        {
            if (list == null || list.Count < 1)
            {
                return BadDistance;
            }

            float minDist = BadDistance;
            for (int l = 0; l < list.Count; l++)
            {
                float newDist = DistanceFromPointToLineSegment(list[l], px, py);
                if (minDist < 0 || newDist < minDist)
                {
                    minDist = newDist;
                }
            }

            return minDist;

        }

        public static MyPoint2 GetClosestPoint2(List<MyPoint2> points, MyPoint2 newPoint, double p = 2)
        {
            if (points == null || points.Count < 1 || newPoint == null)
            {
                return null;
            }

            p = MathUtils.Clamp(0.0001f, p, 10000f);

            double minDist = MaxDistance;
            MyPoint2 closestPoint = null;
            foreach (MyPoint2 pt in points)
            {

                double[] offsets = new double[2];
                offsets[0] = pt.X - newPoint.X;
                offsets[1] = pt.Y - newPoint.Y;

                double dist = MathUtils.LPNorm(offsets, p);

                if (dist < minDist)
                {
                    minDist = dist;
                    closestPoint = pt;
                }
            }

            return closestPoint;
        }




        public static double GetMinDistance2(List<MyPoint2> points, MyPoint2 newPoint, double p = 2)
        {
            MyPoint2 closestPt = GetClosestPoint2(points, newPoint, p);
            if (closestPt == null || newPoint == null)
            {
                return MaxDistance;
            }

            double[] offsets = new double[2];
            offsets[0] = closestPt.X - newPoint.X;
            offsets[1] = closestPt.Y - newPoint.Y;
            return MathUtils.LPNorm(offsets, p);


        }
    }
}
