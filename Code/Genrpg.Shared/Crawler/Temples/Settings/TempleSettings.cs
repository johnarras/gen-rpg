using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    [MessagePackObject]
    public class TempleSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long HealingCostPerLevel { get; set; } = 10;
        [Key(2)] public long StatusEffectCostPerLevel { get; set; } = 100;
        [Key(3)] public long MaxCostLevel { get; set; } = 25;
    }


    [MessagePackObject]
    public class TempleSettingsLoader : NoChildSettingsLoader<TempleSettings> { }



    [MessagePackObject]
    public class TempleSettingsMapper : NoChildSettingsMapper<TempleSettings> { }
}
