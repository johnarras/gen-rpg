using Assets.Scripts.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class OutdoorCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Outdoors; }
    }
}
