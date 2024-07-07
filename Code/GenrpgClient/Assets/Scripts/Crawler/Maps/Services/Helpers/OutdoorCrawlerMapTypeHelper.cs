using Assets.Scripts.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class OutdoorCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Outdoors; }
    }
}
