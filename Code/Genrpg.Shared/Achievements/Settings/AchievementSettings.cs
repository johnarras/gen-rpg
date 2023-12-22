using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Achievements.Settings
{
    [MessagePackObject]
    public class AchievementSettings : ParentSettings<AchievementType>
    {
        [Key(0)] public override string Id { get; set; }

        public AchievementType GetAchievementType(long idkey) { return _lookup.Get<AchievementType>(idkey); }
    }

    [MessagePackObject]
    public class AchievementType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public long Category { get; set; }

    }

    [MessagePackObject]
    public class AchievementSettingsApi : ParentSettingsApi<AchievementSettings, AchievementType> { }

    [MessagePackObject]
    public class AchievementSettingsLoader : ParentSettingsLoader<AchievementSettings, AchievementType, AchievementSettingsApi> { }

}
