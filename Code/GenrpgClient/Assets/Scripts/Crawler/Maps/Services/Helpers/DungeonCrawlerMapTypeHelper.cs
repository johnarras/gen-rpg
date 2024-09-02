using Assets.Scripts.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.MapGen.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{

    public class DungeonCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Dungeon; }

        protected override bool IsIndoors() { return true; }
    }
}
