using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using System.Linq;
using Newtonsoft.Json;
using Genrpg.Shared.PlayerFiltering.Settings;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.GameSettings.Settings
{
    [MessagePackObject]
    public class DataOverrideSettings : BaseDataOverrideSettings<DataOverrideGroup>
    {
        [Key(0)] public override string Id { get; set; } 
    }

    [MessagePackObject]
    public class DataOverrideGroup : ChildSettings, IPlayerFilter
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public bool Enabled { get; set; } = true;

        [Key(5)] public long TotalModSize { get; set; }
        [Key(6)] public long MaxAcceptableModValue { get; set; }
        [Key(7)] public long Priority { get; set; }

        [Key(8)] public double MinUserDaysSinceInstall { get; set; }
        [Key(9)] public double MaxUserDaysSinceInstall { get; set; }
        [Key(10)] public long MinLevel { get; set; }
        [Key(11)] public long MaxLevel { get; set; }
        [Key(12)] public long MinPurchaseCount { get; set; }
        [Key(13)] public double MinPurchaseTotal { get; set; }

        [Key(14)] public bool UseDateRange { get; set; }
        [Key(15)] public DateTime StartDate { get; set; }
        [Key(16)] public DateTime EndDate { get; set; }
        [Key(17)] public int RepeatHours { get; set; }
        [Key(18)] public bool RepeatMonthly { get; set; }

        [Key(19)] public string MaxClientVersion { get; set; }
        [Key(20)] public string MinClientVersion { get; set; }

        [Key(21)] public List<DataOverrideItem> Items { get; set; } = new List<DataOverrideItem>();
        [Key(22)] public List<AllowedPlayer> AllowedPlayers { get; set; } = new List<AllowedPlayer>();

        public void OrderSelf()
        {
            Items = Items.OrderBy(x => x.SettingsNameId).ThenBy(x => x.DocId).ToList();
        }

    }

    [MessagePackObject]
    public class DataOverrideItem
    {
        [Key(0)] public bool Enabled { get; set; } = true;
        [Key(1)] public long SettingsNameId { get; set; }
        [Key(2)] public string DocId { get; set; }
        [Key(3)] public string Name { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideItemPriority
    {
        [Key(0)] public long SettingsNameId { get; set; }
        [Key(1)] public string DocId { get; set; }
        [Key(2)] public long Priority { get; set; }
        [Key(3)] public string Name { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideSettingsApi : ParentSettingsApi<DataOverrideSettings, DataOverrideGroup> { }
    [MessagePackObject]
    public class DataOverrideLoader : ParentSettingsLoader<DataOverrideSettings, DataOverrideGroup> { }

    [MessagePackObject]
    public class DataOverrideSettingsMapper : ParentSettingsMapper<DataOverrideSettings, DataOverrideGroup, DataOverrideSettingsApi> { }


}
