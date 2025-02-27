using Genrpg.Shared.Crawler.Monsters.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents
{
    public class RefreshUnitStatus
    {
        public CrawlerUnit Unit { get; set; }
        public long ElementTypeId { get; set; }
    }
}
