using Genrpg.Shared.Characters.PlayerData;
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
        [Key(1)] public double MinHitToDefenseRatio { get; set; }
        [Key(2)] public double MaxHitToDefenseRatio { get; set; }
        [Key(3)] public double DefendDamageScale { get; set; }
        [Key(4)] public double GuardianDamageScale { get; set; }
        [Key(5)] public double TauntDamageScale { get; set; }
        [Key(6)] public long MaxGroupSize { get; set; }
        [Key(7)] public double LuckCritChanceAtLevel { get; set; }
        [Key(8)] public double MaxLuckCritRatio { get; set; }
        [Key(9)] public double VulnerabilityDamageMult { get; set; }
        [Key(10)] public double HiddenSingleTargetCritPercent { get; set; }
        [Key(11)] public double ResistAddCritChance { get; set; }
        [Key(12)] public double VulnAddCritChance { get; set; }
        [Key(13)] public double GuaranteedHitDefenseRatio { get; set; }
        [Key(14)] public int BaseMonsterMinDam { get; set; }
        [Key(15)] public int BaseMonsterMaxDam { get; set; }
        [Key(16)] public double MinMonsterDamPerLevel { get; set; }
        [Key(17)] public double MaxMonsterDamPerLevel { get; set; }
        [Key(18)] public double MonsterScalingPerLevel { get; set; }
        [Key(19)] public double RandomEncounterChance { get; set; }
        [Key(20)] public int MovesBetweenEncounters { get; set; }

        [Key(21)] public int BaseMonsterMinHealth { get; set; }
        [Key(22)] public int BaseMonsterMaxHealth { get; set; }
        [Key(23)] public double MinMonsterHealthPerLevel { get; set; }
        [Key(24)] public double MaxMonsterHealthPerLevel { get; set; }
    }


    [MessagePackObject]
    public class CrawlerCombatSettingsLoader : NoChildSettingsLoader<CrawlerCombatSettings> { }



    [MessagePackObject]
    public class CrawlerCombatSettingsMapper : NoChildSettingsMapper<CrawlerCombatSettings> { }
}
