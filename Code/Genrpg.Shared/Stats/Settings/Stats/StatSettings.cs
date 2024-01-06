using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using System.Linq;
using Genrpg.Shared.Stats.Constants;

namespace Genrpg.Shared.Stats.Settings.Stats
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

        public List<StatType> GetPowerStats()
        {
            return _data.Where(x => x.IdKey <= StatConstants.MaxMutableStatTypeId &&
            x.IdKey > 0 && x.IdKey != StatTypes.Health).ToList();
        }
    }

    [MessagePackObject]
    public class StatSettingsApi : ParentSettingsApi<StatSettings, StatType> { }
    [MessagePackObject]
    public class StatSettingsLoader : ParentSettingsLoader<StatSettings, StatType, StatSettingsApi> { }
}
