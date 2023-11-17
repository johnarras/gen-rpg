using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Stats.Entities
{
    [MessagePackObject]
    public class StatSettings : ParentSettings<StatType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int StatConstantUnitMultiple { get; set; }

        public StatType GetStatType(long idkey)
        {
            return _lookup.Get<StatType>(idkey);
        }
    }

    [MessagePackObject]
    public class StatSettingsApi : ParentSettingsApi<StatSettings, StatType> { }
    [MessagePackObject]
    public class StatSettingsLoader : ParentSettingsLoader<StatSettings, StatType, StatSettingsApi> { }
}
