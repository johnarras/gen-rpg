using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Activities.Settings
{
    [MessagePackObject]
    public class ActivitySettings : ParentSettings<Activity>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class Activity : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long ResetDays { get; set; }
        [Key(8)] public long DurationDays { get; set; }
        [Key(9)] public long ResetDayOffset { get; set; }

        [Key(10)] public long BaseCost { get; set; }
        [Key(11)] public long IncrementalCost { get; set; }
        [Key(12)] public long MaxLevel { get; set; }
        [Key(13)] public long BaseReward { get; set; }
        [Key(14)] public long IncrementalReward { get; set; }

        public long GetCost(long nextLevel)
        {
            return GetTotal(BaseCost, IncrementalCost, nextLevel);
        }

        private long GetTotal(long baseAmount, long increment, long level)
        {
            long total = baseAmount + (level - 1) * increment;

            if (level > MaxLevel/2)
            {
                total += increment * (MaxLevel / 2 - level - 1);
            }
            return total;
        }

        public long GetReward(long level)
        {
            return GetTotal(BaseReward, IncrementalReward, level-1);
        }

    }

    [MessagePackObject]
    public class ActivitySettingsApi : ParentSettingsApi<ActivitySettings, Activity> { }

    [MessagePackObject]
    public class ActivitySettingsLoader : ParentSettingsLoader<ActivitySettings, Activity> { }

    [MessagePackObject]
    public class ActivitySettingsMapper : ParentSettingsMapper<ActivitySettings, Activity, ActivitySettingsApi> { }
}
