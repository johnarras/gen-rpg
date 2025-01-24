using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Monsters.Settings
{
    [MessagePackObject]
    public class CrawlerMonsterSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int BaseMinDam { get; set; }
        [Key(2)] public int BaseMaxDam { get; set; }
        [Key(3)] public double MinDamPerLevel { get; set; }
        [Key(4)] public double MaxDamPerLevel { get; set; }
        [Key(5)] public double ScalingPerLevel { get; set; }

        [Key(6)] public int BaseMinHealth { get; set; }
        [Key(7)] public int BaseMaxHealth { get; set; }
        [Key(8)] public double MinHealthPerLevel { get; set; }
        [Key(9)] public double MaxHealthPerLevel { get; set; }


        [Key(10)] public long ManaPerLevel { get; set; }
    }


    [MessagePackObject]
    public class CrawlerMonsterSettingsLoader : NoChildSettingsLoader<CrawlerMonsterSettings> { }



    [MessagePackObject]
    public class CrawlerMonsterSettingsMapper : NoChildSettingsMapper<CrawlerMonsterSettings> { }
}
