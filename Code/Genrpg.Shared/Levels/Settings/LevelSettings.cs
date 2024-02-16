using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Levels.Settings
{
    [MessagePackObject]
    public class LevelInfo : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public List<SpawnResult> RewardList { get; set; }

        [Key(7)] public long CurrExp { get; set; }
        [Key(8)] public float MobCount { get; set; }
        [Key(9)] public long MobExp { get; set; }
        [Key(10)] public float QuestCount { get; set; }
        [Key(11)] public long QuestExp { get; set; }
        [Key(12)] public long KillMoney { get; set; }

        [Key(13)] public int StatAmount { get; set; }
        [Key(14)] public int MonsterStatScale { get; set; }

        [Key(15)] public int AbilityPoints { get; set; }

        [Key(16)] public string Art { get; set; }


        public LevelInfo()
        {
            RewardList = new List<SpawnResult>();
        }
    }

    [MessagePackObject]
    public class LevelSettings : ParentSettings<LevelInfo>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int MaxLevel { get; set; }
    }

    [MessagePackObject]
    public class LevelSettingsApi : ParentSettingsApi<LevelSettings, LevelInfo> { }

    [MessagePackObject]
    public class LevelSettingsLoader : ParentSettingsLoader<LevelSettings, LevelInfo, LevelSettingsApi> { }

}
