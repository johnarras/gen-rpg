using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Entities
{
    public class MapCellDetail
    {
        public int X { get; set; }
        public int Z { get; set; }
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long Value { get; set; }
        public int ToX { get; set; }
        public int ToZ { get; set; }
    }
}
