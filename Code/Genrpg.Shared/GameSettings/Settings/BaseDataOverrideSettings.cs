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

namespace Genrpg.Shared.GameSettings.Settings
{
    [MessagePackObject]
    public abstract class BaseDataOverrideSettings<TChild> : ParentSettings<TChild> where TChild : ChildSettings, IPlayerFilter, new()
    {
        // Temp internal data to make updating configs cheaper.
        [JsonIgnore][IgnoreMember] public DateTime PrevUpdateTime { get; set; } = DateTime.MinValue;
        [JsonIgnore][IgnoreMember] public DateTime NextUpdateTime { get; set; } = DateTime.MaxValue;
        [JsonIgnore][IgnoreMember] public List<DateTime> AllUpdateTimes { get; set; } = new List<DateTime>();

        public override void SetData(List<TChild> data)
        {
            data = data.OrderBy(x => x.IdKey).ToList();
            foreach (TChild group in data)
            {
                group.OrderSelf();
            }

            AllUpdateTimes = data.Select(x => x.StartDate).Union(data.Select(x => x.EndDate)).Distinct().OrderBy(x => x).ToList();

            SetPrevNextUpdateTimes();

            base.SetData(data);
        }


        private readonly object _updateTimeLock = new object();
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
}

