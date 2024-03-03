using Assets.Scripts.Crawler.Maps.Entities;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class UnityMapCell
    {
        public int X { get; set; }
        public int Z { get; set; }
        public List<MapCellDetail> Details { get; set; } = new List<MapCellDetail>();
        public GEntity Content { get; set; }
    }

}
