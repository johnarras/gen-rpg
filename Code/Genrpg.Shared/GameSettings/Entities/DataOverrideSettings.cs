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
using Newtonsoft.Json;

namespace Genrpg.Shared.GameSettings.Entities
{
    [MessagePackObject]
    public class DataOverrideSettings : ParentSettings<DataOverrideGroup>
    {
        [Key(0)] public override string Id { get; set; }


        // Temp internal data to make updating configs cheaper.
        [JsonIgnore][IgnoreMember] public DateTime PrevUpdateTime { get; set; } = DateTime.MinValue;
        [JsonIgnore][IgnoreMember] public DateTime NextUpdateTime { get; set; } = DateTime.MaxValue;
        [JsonIgnore][IgnoreMember] public List<DateTime> AllUpdateTimes { get; set; } = new List<DateTime>();

        public override void SetData(List<DataOverrideGroup> data)
        {
            data = data.OrderBy(x => x.IdKey).ToList();

            foreach (DataOverrideGroup group in data)
            {
                group.Items = group.Items.OrderBy(x => x.SettingId).ThenBy(x => x.DocId).ToList();
            }

            AllUpdateTimes = data.Select(x => x.StartDate).Union(data.Select(x => x.EndDate)).Distinct().OrderBy(x => x).ToList();

            SetPrevNextUpdateTimes();

            base.SetData(data);
        }


        private object _updateTimeLock = new object();
        public void SetPrevNextUpdateTimes()
        {
            lock (_updateTimeLock)
            {
                // We are using DateTime.UtcNow to set these times, but even though all
                // servers will have slightly different updateTimes they check, if there's
                // two changes close together, a few servers may update once, and some
                // may update twice and players who update in between may download
                // slightly different data, but it will settle once the time goes past
                // the second update time.
                DateTime updateTime = DateTime.UtcNow;
                List<DateTime> updates = AllUpdateTimes;

                if (updates.Any(x => x <= updateTime))
                {
                    PrevUpdateTime = updates.Last(x => x <= updateTime);
                }
                else
                {
                    PrevUpdateTime = DateTime.MinValue;
                }

                if (updates.Any(x => x > updateTime))
                {
                    NextUpdateTime = updates.First(x => x > updateTime);
                }
                else
                {
                    NextUpdateTime = DateTime.MaxValue;
                }
            }
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
