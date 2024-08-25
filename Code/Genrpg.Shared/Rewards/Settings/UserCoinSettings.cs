using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Rewards.Settings
{
    [MessagePackObject]
    public class RewardSourceSettings : ParentSettings<RewardSourceType>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class RewardSourceType : ChildSettings, IIndexedGameItem
    {
        public const int None = 0;
        public const int Doubloons = 1;


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string PluralName { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

    }
    [MessagePackObject]
    public class RewardSourceSettingsApi : ParentSettingsApi<RewardSourceSettings, RewardSourceType> { }
    [MessagePackObject]
    public class UnitCoinSettingsLoader : ParentSettingsLoader<RewardSourceSettings, RewardSourceType> { }

    [MessagePackObject]
    public class RewardSourceSettingsMapper : ParentSettingsMapper<RewardSourceSettings, RewardSourceType, RewardSourceSettingsApi> { }
}
