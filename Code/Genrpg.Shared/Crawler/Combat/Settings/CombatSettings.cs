using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    [MessagePackObject]
    public class CrawlerCombatSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double MinHitToDefenseRatio { get; set; } = 0.25f;
        [Key(2)] public double MaxHitToDefenseRatio { get; set; } = 1.25f;
        [Key(3)] public double DefendDamageScale { get; set; } = 0.5f;
        [Key(4)] public double TauntDamageScale { get; set; } = 0.25f;
        [Key(5)] public long MaxGroupSize { get; set; } = 99;
    }


    [MessagePackObject]
    public class CrawlerCombatSettingsLoader : NoChildSettingsLoader<CrawlerCombatSettings> { }



    [MessagePackObject]
    public class CrawlerCombatSettingsMapper : NoChildSettingsMapper<CrawlerCombatSettings> { }
}
