using Assets.Scripts.Buildings;
using Assets.Scripts.Crawler.Maps.GameObjects;
using Genrpg.Shared.Buildings.Settings;

namespace Assets.Scripts.Crawler.Maps.Loading
{
    public class CrawlerObjectLoadData
    {
        public ClientMapCell MapCell { get; set; }
        public BuildingType BuildingType { get; set; }
        public long Angle { get; set; }
        public CrawlerMapRoot MapRoot { get; set; }
        public long Seed { get; set; }
        public BuildingMats Mats { get; set; }
    }

}
