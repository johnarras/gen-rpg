using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Stats.Settings
{
    [MessagePackObject]
    public class CrawlerStatSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int StartStat { get; set; } = 10;
    }


    [MessagePackObject]
    public class CrawlerStatSettingsLoader : NoChildSettingsLoader<CrawlerStatSettings> { }



    [MessagePackObject]
    public class CrawlerStatSettingsMapper : NoChildSettingsMapper<CrawlerStatSettings> { }
}
