using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System.Transactions;

namespace Genrpg.Shared.Crawler.MapGen.Settings
{
    [MessagePackObject]
    public class CrawlerMapSettings : ParentSettings<CrawlerMapType>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class CrawlerMapType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long MinWidth { get; set; } = 15;
        [Key(8)] public long MaxWidth { get; set; } = 25;
        [Key(9)] public long MinHeight { get; set; } = 15;
        [Key(10)] public long MaxHeight { get; set; } = 25;
        [Key(11)] public long MinFloors { get; set; } = 1;
        [Key(12)] public long MaxFloors { get; set; } = 1;
        [Key(13)] public double QuestItemEntranceUnlockChance { get; set; } = 0;
        [Key(14)] public double RiddleUnlockChance { get; set; } = 0;
    }

    [MessagePackObject]
    public class CrawlerMapSettingsApi : ParentSettingsApi<CrawlerMapSettings, CrawlerMapType> { }
    [MessagePackObject]
    public class CrawlerMapSettingsLoader : ParentSettingsLoader<CrawlerMapSettings, CrawlerMapType> { }

    [MessagePackObject]
    public class CrawlerMapSettingsMapper : ParentSettingsMapper<CrawlerMapSettings, CrawlerMapType, CrawlerMapSettingsApi> { }

}
