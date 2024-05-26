using Genrpg.Shared.Interfaces;
using MessagePack;
using System;

namespace Genrpg.Shared.Utils
{

    public interface IRandom : IInjectable
    {
        int Next();
        long NextLong();
        int Next(int maxVal); 
        int Next(int minValue, int maxValue);
        long NextLong(long minValue, long maxValue);
        double NextDouble();
    }

    [MessagePackObject]
    public class MyRandom : IRandom
    {

        private System.Random _rand;
        public MyRandom(long seed)
        {
            _rand = new System.Random((int)seed);
        }

        public MyRandom()
        {
            Reset();
        }

        private void Reset()
        {
            _rand = new System.Random((int)(DateTime.UtcNow.Ticks % 1000000000));
        }

        public int Next()
        {
            int val = _rand.Next();
            if (val == 0)
            {
                //Reset();
            }
            return val;
        }


        public long NextLong()
        {
            byte[] bytes = new byte[8];
            _rand.NextBytes(bytes);
            long val = BitConverter.ToInt64(bytes, 0);
            if (val == 0)
            {
                //Reset();
            }
            return val >= 0 ? val : -val;
        }

        public int Next(int maxValue)
        {
            if (maxValue < 1)
            {
                return 0;
            }

            return Next() % maxValue;
        }

        public long NextLong(long maxValue)
        {
            if (maxValue < 1)
            {
                return 0;
            }

            return NextLong() % maxValue;
        }

        /// <summary>
        /// Returns a MyRandom number between minValue and maxValue-1
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxValue)
        {
            return minValue + Next(maxValue - minValue + 1);
        }

        public long NextLong(long minValue, long maxValue)
        {
            return minValue + NextLong(maxValue - minValue + 1);
        }

        public double NextDouble()
        {
            return Next() * (1.0 / 0x7FFFFFFF);
        }
    }
}
