using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Utils
{
    public static class RandomUtils
    {
        public static T GetRandomElement<T>(IEnumerable<T> list,IRandom rand) where T : IWeightedItem
        {
            double chanceSum = list.Sum(x => x.Weight);

            double chanceChosen = rand.NextDouble() * chanceSum;

            foreach (T t in list)
            {
                chanceChosen -= t.Weight;
                if (chanceChosen <= 0)
                {
                    return t;
                }
            }
            return default(T);
        }
    }
}
