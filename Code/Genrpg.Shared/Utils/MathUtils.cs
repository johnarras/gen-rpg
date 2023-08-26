using MessagePack;
using System;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class MathUtils
    {
        public static int Clamp(int min, int mid, int max)
        {
            if (mid < min)
            {
                return min;
            }

            if (mid > max)
            {
                return max;
            }

            return mid;
        }
        public static long Clamp(long min, long mid, long max)
        {
            if (mid < min)
            {
                return min;
            }

            if (mid > max)
            {
                return max;
            }

            return mid;
        }

        public static float Clamp(float min, float mid, float max)
        {
            if (mid < min)
            {
                return min;
            }

            if (mid > max)
            {
                return max;
            }

            return mid;
        }


        public static double Clamp(double min, double mid, double max)
        {
            if (mid < min)
            {
                return min;
            }

            if (mid > max)
            {
                return max;
            }

            return mid;
        }


        public static float FloatRange(double minVal, double maxVal, MyRandom rand)
        {
            if (rand == null)
            {
                return (float)(minVal + maxVal / 2);
            }

            return (float)(minVal + rand.NextDouble() * (maxVal - minVal));
        }

        /// <summary>
        /// Pick a random range that generally goes from midVal-scaleDelta to midVal+scaleDelta, but
        /// give a certain number of chances (scaleTimes) to roll a number less than (scaleChance) to 
        /// increase the size of the random range by scaleDelta again.
        /// </summary>
        /// <param name="midval"></param>
        /// <param name="rand"></param>
        /// <param name="scaleTimes"></param>
        /// <param name="scaleChance"></param>
        /// <param name="scaleDelta"></param>
        /// <returns></returns>
        public static float ScaledRange(float midval, MyRandom rand, int scaleTimes, double scaleChance)
        {
            if (rand == null)
            {
                return midval;
            }

            int totalScaleTimes = 0;
            for (int i = 0; i < scaleTimes; i++)
            {
                if (rand.NextDouble() < scaleChance)
                {
                    totalScaleTimes++;
                }
                else
                {
                    break;
                }
            }

            if (rand.NextDouble() < 0.5f)
            {
                return FloatRange(0.5f / (1 + totalScaleTimes), 1.0f, rand) * midval;
            }
            else
            {
                return FloatRange(1.0f, totalScaleTimes + 2, rand) * midval;
            }
        }


        public static int IntRange(int minVal, int maxVal, MyRandom rand)
        {
            if (rand == null || minVal >= maxVal)
            {
                return (minVal + maxVal) / 2;
            }

            return minVal + rand.Next() % (maxVal - minVal + 1);
        }
        public static long LongRange(long minVal, long maxVal, MyRandom rand)
        {
            if (rand == null || minVal >= maxVal)
            {
                return (minVal + maxVal) / 2;
            }

            return minVal + rand.NextLong() % (maxVal - minVal + 1);
        }

        public static float Sqrt(float val)
        {
            if (val < 0)
            {
                return 0;
            }

            return (float)Math.Sqrt(val);
        }

        public static double Sqrt(double val)
        {
            if (val < 0)
            {
                return 0;
            }

            return Math.Sqrt(val);
        }
        public static float Abs(float val)
        {
            return (float)Math.Abs(val);
        }

        public static double Abs(double val)
        {
            return Math.Abs(val);
        }

        public static float Max(float val1, float val2)
        {
            return (float)Math.Max(val1, val2);
        }

        public static double Max(double val1, double val2)
        {
            return Math.Max(val1, val2);
        }


        public static float Min(float val1, float val2)
        {
            return (float)Math.Min(val1, val2);
        }

        public static double Min(double val1, double val2)
        {
            return Math.Min(val1, val2);
        }


        /// <summary>
        /// Returns a function that smoothly transitions from 0 to 1 within 3 ranges.
        /// 
        /// 1. If the currentDistance is < innerRadius, it returns 0.
        /// 2. If it's less than the midpoint between innerRadius and maxRadius it returns a quadratic
        /// starting from 0 and rising to 0.5.
        /// 3. If it's more than the midpoint, it returns a negative quadratic that starts at 0.5 and
        /// rises to 1 near maxRadius.
        /// </summary>
        /// <param name="innerRadius"></param>
        /// <param name="maxRadius"></param>
        /// <param name="currentDistance"></param>
        public static float GetSmoothScalePercent(float innerRadius, float maxRadius, float currentDistance)
        {
            if (innerRadius < 0 || innerRadius >= maxRadius)
            {
                return 0;
            }

            currentDistance = Clamp(0, currentDistance, maxRadius);

            if (currentDistance <= innerRadius)
            {
                return 0;
            }

            float midRadius = (innerRadius + maxRadius) / 2;

            if (currentDistance <= midRadius)
            {
                float diff = (currentDistance - innerRadius) / (midRadius - innerRadius);
                return 0.5f * diff * diff;
            }
            else
            {
                float diff = (maxRadius - currentDistance) / (maxRadius - midRadius);
                return 1.0f - 0.5f * diff * diff;
            }


        }

        public static double LPNorm(double[] vals, double p)
        {
            if (vals == null || vals.Length < 1 || p < 0.0001f || p > 100000)
            {
                return 0.0f;
            }
            double total = 0;
            foreach (double val in vals)
            {
                total += Math.Pow(Math.Abs(val), p);
            }

            return Math.Pow(total, 1 / p);
        }

        public static double LPNorm(int[] vals, double p)
        {
            if (vals == null)
            {
                return 0.0f;
            }

            double[] newVals = new double[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                newVals[i] = vals[i];
            }
            return LPNorm(newVals, p);
        }

        public static double LPNorm(float[] vals, double p)
        {
            if (vals == null)
            {
                return 0.0f;
            }

            double[] newVals = new double[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                newVals[i] = vals[i];
            }
            return LPNorm(newVals, p);
        }


        public static float SeedFloatRange(long seed, int mult, float minval, float maxval, int steps = 101)
        {
            if (steps < 1 || minval >= maxval)
            {
                return minval;
            }

            return minval + (maxval - minval) * (seed * mult % steps) / (1.0f * steps);
        }

        /// <summary>
        /// This creates something similar to an sshaped curve on the unit interval between height 0 and 1 using piecewise quadratics.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="xInUnityInterval"></param>
        /// <returns></returns>
        public static float QuadraticSShaped(float xInUnityInterval)
        {
            xInUnityInterval = Clamp(0, xInUnityInterval, 1);

            if (xInUnityInterval <= 0.5f)
            {
                return 2 * xInUnityInterval * xInUnityInterval;
            }
            else
            {
                return 1 - 2 * (1 - xInUnityInterval) * (1 - xInUnityInterval);
            }
        }


        public static long RoundToNiceValue(long value)
        {
            long mult = 1;

            if (value <= 10)
            {
                return value;
            }

            if (value <= 1000)
            {
                value = value + 2 - (value + 2) % 5;
                return value;
            }

            while (value >= 1000)
            {
                value /= 10;
                mult *= 10;
            }

            value += 2;
            value -= value % 5;

            value *= mult;

            return value;
        }


    }
}
