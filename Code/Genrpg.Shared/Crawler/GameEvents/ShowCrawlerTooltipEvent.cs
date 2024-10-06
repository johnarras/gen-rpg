using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class ShowCrawlerTooltipEvent
    {
        public List<string> Lines { get; set; } = new List<string>();
    }
}
