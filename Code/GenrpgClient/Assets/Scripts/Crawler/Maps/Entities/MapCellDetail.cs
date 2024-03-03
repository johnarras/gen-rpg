using System;
using System.Collections.Generic;
using System.Linq;
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
        public long Quantity { get; set; }
    }

}
