using Genrpg.Shared.Crawler.Maps.Constants;

namespace Assets.Scripts.Crawler.Maps.Services.GenerateMaps
{
    public class DungeonMapGenHelper : BaseDungeonMapGenHelper
    {
        public override long GetKey() { return CrawlerMapTypes.Dungeon; }
    }
}
