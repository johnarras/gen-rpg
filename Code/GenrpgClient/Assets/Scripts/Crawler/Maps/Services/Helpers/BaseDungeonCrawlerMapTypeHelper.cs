
using Genrpg.Shared.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{

    public abstract class BaseDungeonCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        protected override bool IsIndoors() { return true; }
    }
}
