using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Units.Entities
{
    [MessagePackObject]
    public class UnitUtils
    {
        public static bool TurnTowardPosition(Unit unit, float targetx, float targetz, float maxRotation = 360)
        {

            float cx = unit.X;
            float cz = unit.Z;

            float dx = targetx - cx;
            float dz = targetz - cz;

            if (Math.Abs(dx) < 0.01f && Math.Abs(dz) < 0.01f)
            {
                return true;
            }

            float targetRot = (float)(180.0 / Math.PI * Math.Atan2(dx, dz));

            while (unit.Rot > 180)
            {
                unit.Rot -= 360;
            }

            while (unit.Rot < -180)
            {
                unit.Rot += 360;
            }

            while (targetRot < unit.Rot - 180)
            {
                targetRot += 360;
            }

            while (targetRot > unit.Rot + 180)
            {
                targetRot -= 360;
            }
            float absRotDiff = Math.Abs(targetRot - unit.Rot);

            float rotDiff = targetRot - unit.Rot;

            if (absRotDiff > maxRotation)
            {
                if (targetRot < unit.Rot)
                {
                    unit.Rot -= maxRotation;
                }
                else if (targetRot > unit.Rot)
                {
                    unit.Rot += maxRotation;
                }
            }
            else
            {
                unit.Rot = targetRot;
            }

            while (unit.Rot > 180)
            {
                unit.Rot -= 360;
            }

            while (unit.Rot < -180)
            {
                unit.Rot += 360;
            }

            return true;
        }


        public static float DistanceTo(Unit unit, Unit target)
        {
            if (target == null)
            {
                return UnitConstants.ErrorDistance;
            }
            return DistanceTo(unit, target.X, target.Y, target.Z);
        }

        public static float DistanceTo(Unit unit, float x, float y, float z)
        {
            float dx = unit.X - x;
            float dz = unit.Z - z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }

        public static bool IsFacing(Unit unit, Unit target)
        {
            return InnerIsFacing(unit, AngleToward(unit, target));
        }

        public static bool IsFacing(Unit unit, float x, float y, float z)
        {
            return InnerIsFacing(unit, AngleToward(unit, x, y, z));
        }

        protected static bool InnerIsFacing(Unit unit, float angle)
        {

            if (angle < -10000 || angle >= 10000)
            {
                return false;
            }

            float rot = unit.Rot;

            while (angle - rot > 180)
            {
                angle -= 360;
            }

            while (angle - rot < -180)
            {
                angle += 360;
            }

            return Math.Abs(angle - rot) < 90;
        }

        public static float AngleToward(Unit unit, float x, float y, float z)
        {

            float dx = unit.X - x;
            float dz = unit.Z - z;

            float angle = (float)Math.Atan2(dz, dx);

            return angle;
        }

        public static float AngleToward(Unit unit, Unit target)
        {

            if (target == null)
            {
                return -100000;
            }
            return AngleToward(unit, target.X, target.Y, target.Z);
        }

        public static void SetTargetPos(Unit unit, Unit target, float gap)
        {

            float tdx = target.X - unit.X;
            float tdz = target.Z - unit.Z;

            float tdist = (float)Math.Sqrt(tdx * tdx + tdz * tdz);

            if (tdist > gap)
            {
                float ndx = tdx / tdist;
                float ndz = tdz / tdist;
                unit.ToX = target.X - ndx * gap;
                unit.ToZ = target.Z - ndz * gap;
            }
            else
            {
                unit.ToX = unit.X;
                unit.ToZ = unit.Z;
            }
        }
    }
}
