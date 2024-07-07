using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.Entities
{
    public class MapQuestItem
    {
        public long QuestItemId { get; set; }
        public long Quantity { get; set; }
        public long FoundInMapId { get; set; }
    }
}
