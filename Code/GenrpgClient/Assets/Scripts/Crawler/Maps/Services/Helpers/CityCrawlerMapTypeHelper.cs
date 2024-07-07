
using Assets.Scripts.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class CityCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.City; }
    }
}
