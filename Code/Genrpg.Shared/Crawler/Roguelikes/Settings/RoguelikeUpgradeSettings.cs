using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;

namespace Genrpg.Shared.Crawler.Roguelikes.Settings
{
    [MessagePackObject]
    public class RoguelikeUpgradeSettings : ParentSettings<RoguelikeUpgrade>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long BasePointsPerLevel { get; set; }
        [Key(2)] public double ExtraPointsPerLevel { get; set; }

    }

    [MessagePackObject]
    public class RoguelikeUpgrade : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public long MaxTier { get; set; }
        [Key(9)] public double BonusPerTier { get; set; }
        [Key(10)] public long BasePointCost { get; set; }


    }


    [MessagePackObject]
    public class RoguelikeUpgradeSettingsApi : ParentSettingsApi<RoguelikeUpgradeSettings, RoguelikeUpgrade> { }
    [MessagePackObject]
    public class RoguelikeUpgradeSettingsLoader : ParentSettingsLoader<RoguelikeUpgradeSettings, RoguelikeUpgrade> { }

    [MessagePackObject]
    public class RoguelikeUpgradeSettingsMapper : ParentSettingsMapper<RoguelikeUpgradeSettings, RoguelikeUpgrade, RoguelikeUpgradeSettingsApi> { }
}
