using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedDungeonAsset
    {
        public WeightedDungeonAsset()
        {
            Weight = 1000;
        }


        public int Weight = 1000;
        public DungeonAsset Asset;
    }
}
