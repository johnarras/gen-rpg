using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Crawler.Maps.GameObjects
{
    public class ClientMapCell
    {
        public int X { get; set; }
        public int Z { get; set; }
        public List<MapCellDetail> Details { get; set; } = new List<MapCellDetail>();
        public object Content { get; set; }

    }

}
