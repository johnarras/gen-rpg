using Assets.Scripts.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.Helpers
{

    public class DungeonCrawlerMapTypeHelper : BaseCrawlerMapTypeHelper
    {
        public override ECrawlerMapTypes GetKey() { return ECrawlerMapTypes.Dungeon; }

        protected override bool IsIndoors() { return true; }
    }
}
