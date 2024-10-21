using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.GameEvents
{
    [MessagePackObject]
    public class ShowCrawlerTooltipEvent
    {
        [Key(0)] public List<string> Lines { get; set; } = new List<string>();
    }
}
