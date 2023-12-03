using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using System.Linq;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class DataOverrideSettings : ParentSettings<DataOverrideGroup>
    {
        [Key(0)] public override string Id { get; set; }

        public override void SetData(List<DataOverrideGroup> data)
        {
            data = data.OrderBy(x => x.IdKey).ToList();

            foreach (DataOverrideGroup group in data)
            {
                group.Items = group.Items.OrderBy(x => x.SettingId).ThenBy(x => x.DocId).ToList();
            }

            base.SetData(data);
        }
    }

    [MessagePackObject]
    public class DataOverrideGroup : ChildSettings, IPlayerFilter
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }

        [Key(4)] public long TotalModSize { get; set; }
        [Key(5)] public long MaxAcceptableModValue { get; set; }
        [Key(6)] public long Priority { get; set; }

        [Key(7)] public double MinUserDaysSinceInstall { get; set; }
        [Key(8)] public double MaxUserDaysSinceInstall { get; set; }
        [Key(9)] public long MinLevel { get; set; }
        [Key(10)] public long MaxLevel { get; set; }
        [Key(11)] public long MinPurchaseCount { get; set; }
        [Key(12)] public double MinPurchaseTotal { get; set; }

        [Key(13)] public bool UseDateRange { get; set; }
        [Key(14)] public DateTime StartDate { get; set; }
        [Key(15)] public DateTime EndDate { get; set; }

        [Key(16)] public List<DataOverrideItem> Items { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideItem
    {
        [Key(0)] public string SettingId { get; set; }
        [Key(1)] public string DocId { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideItemPriority
    {
        [Key(0)] public string SettingId { get; set; }
        [Key(1)] public string DocId { get; set; }
        [Key(2)] public long Priority { get; set; }
    }

    [MessagePackObject]
    public class DataOverrideApi : ParentSettingsApi<DataOverrideSettings, DataOverrideGroup> { }
    [MessagePackObject]
    public class DataOverrideLoader : ParentSettingsLoader<DataOverrideSettings, DataOverrideGroup, DataOverrideApi> { }
    
}
