using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Loot.Settings
{
    [MessagePackObject]
    public class CrawlerLootSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long BaseLootCost { get; set; } = 10;
        [Key(2)] public double WeaponMult { get; set; } = 2;
        [Key(3)] public double TwoHandWeaponMult { get; set; } = 1.5f;
        [Key(4)] public double ProcMult { get; set; } = 2;
        [Key(5)] public double EffectMult { get; set; } = 2;
        [Key(6)] public long MaxLootItems { get; set; } = 4;
        [Key(7)] public double ItemChancePerMonster { get; set; } = 0.2f;
    }

    [MessagePackObject]
    public class CrawlerLootSettingsLoader : NoChildSettingsLoader<CrawlerLootSettings> { }



    [MessagePackObject]
    public class CrawlerLootSettingsMapper : NoChildSettingsMapper<CrawlerLootSettings> { }
}
