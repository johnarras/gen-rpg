using MessagePack;
using System;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class MyRandom
    {

        private System.Random _rand;
        public MyRandom(long seed)
        {
            _rand = new System.Random((int)seed);
        }

        public MyRandom()
        {
            _rand = new System.Random((int)(DateTime.UtcNow.Ticks));
        }

        public int Next()
        {
            return _rand.Next();
        }


        public long NextLong()
        {
            byte[] bytes = new byte[8];
            _rand.NextBytes(bytes);
            long val = BitConverter.ToInt64(bytes, 0);
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
