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
        [Key(4)] public double GuardianDamageScale { get; set; } = 0.66f;
        [Key(5)] public double TauntDamageScale { get; set; } = 0.25f;
        [Key(6)] public long MaxGroupSize { get; set; } = 99;
        [Key(7)] public double LuckCritChanceAtLevel { get; set; } = 0.05f;
        [Key(8)] public double MaxLuckCritRatio { get; set; } = 2.0f;
        [Key(9)] public double VulnerabilityDamageMult { get; set; } = 3.0f;
        [Key(10)] public double HiddenSingleTargetCritPercent { get; set; } = 100;
        [Key(11)] public double ResistAddCritChance { get; set; } = -200;
        [Key(12)] public double VulnAddCritChance { get; set; } = 10;
    }


    [MessagePackObject]
    public class CrawlerCombatSettingsLoader : NoChildSettingsLoader<CrawlerCombatSettings> { }



    [MessagePackObject]
    public class CrawlerCombatSettingsMapper : NoChildSettingsMapper<CrawlerCombatSettings> { }
}
