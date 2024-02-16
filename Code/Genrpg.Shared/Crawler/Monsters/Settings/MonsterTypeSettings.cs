using Genrpg.Shared.Crawler.Monsters.Constants;
using Genrpg.Shared.Crawler.Monsters.Settings.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Monsters.Settings
{
    [MessagePackObject]
    public class MonsterType : ChildSettings, IIndexedGameItem, IMonsterInfo
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }


        [Key(7)] public long MinLevel { get; set; } = 0;
        [Key(8)] public long MaxLevel { get; set; } = MonsterConstants.MaxLevel;
        [Key(9)] public double Chance { get; set; } = 1.0;
        [Key(10)] public double HpScale { get; set; } = 1.0;
        [Key(11)] public double DamScale { get; set; } = 1.0;
        [Key(12)] public double PrefixChance { get; set; } = 1.0;
        [Key(13)] public double SuffixChance { get; set; } = 1.0;
        [Key(14)] public List<MonsterAffix> Prefixes { get; set; } = new List<MonsterAffix>();
        [Key(15)] public List<MonsterAffix> Suffixes { get; set; } = new List<MonsterAffix>();
        [Key(16)] public List<MonsterAbility> Abilities { get; set; } = new List<MonsterAbility>();

        [Key(17)] public long MonsterCategoryId { get; set; }
     
    }

    [MessagePackObject]
    public class MonsterTypeSettings : ParentSettings<MonsterType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class MonsterTypeSettingsApi : ParentSettingsApi<MonsterTypeSettings, MonsterType> { }
    [MessagePackObject]
    public class MonsterTypeSettingsLoader : ParentSettingsLoader<MonsterTypeSettings, MonsterType, MonsterTypeSettingsApi> { }

}
