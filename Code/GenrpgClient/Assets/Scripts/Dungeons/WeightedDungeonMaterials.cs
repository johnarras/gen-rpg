using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Dungeons
{
    [Serializable]
    public class WeightedDungeonMaterials
    {
        public int Weight = 1000;
        public DungeonMaterials Materials;
    }
}
