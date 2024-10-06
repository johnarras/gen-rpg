
using Genrpg.Shared.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class OutdoorCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Outdoors; }
    }
}
