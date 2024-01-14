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
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Stats.Settings.Stats
{/// <summary>
 /// Stats have current core stats:
 /// Health/Mana/Might/Intellect/Willpower/Agility
 /// </summary>
    [MessagePackObject]
    public class StatType : ChildSettings, IIndexedGameItem
    {


        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Abbrev { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }

        [Key(8)] public int MaxPool { get; set; }
        [Key(9)] public int RegenSeconds { get; set; }
        [Key(10)] public int GenScalePct { get; set; }

    }

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
