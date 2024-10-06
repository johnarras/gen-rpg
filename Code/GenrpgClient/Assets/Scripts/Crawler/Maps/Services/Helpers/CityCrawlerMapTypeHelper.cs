
using Genrpg.Shared.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{
    public class CityCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override long GetKey() { return CrawlerMapTypes.City; }
    }
}
