using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Buildings
{
    [Serializable]
    public class WeightedCrawlerBuilding
    {
        public int Weight;
        public CrawlerBuilding Building;
        public BuildingMats Mats;
    }
}
