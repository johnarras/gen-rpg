using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Settings.DerivedStats
{
    [MessagePackObject]
    public class DerivedStat : ChildSettings
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public override string Name { get; set; }
        [Key(3)] public long FromStatTypeId { get; set; }
        [Key(4)] public long ToStatTypeId { get; set; }
        [Key(5)] public int Percent { get; set; }
    }

    [MessagePackObject]
    public class DerivedStatSettings : ParentSettings<DerivedStat>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class DerivedStatSettingsApi : ParentSettingsApi<DerivedStatSettings, DerivedStat> { }
    [MessagePackObject]
    public class DerivedStatSettingLoader : ParentSettingsLoader<DerivedStatSettings, DerivedStat> { }

    [MessagePackObject]
    public class DerivedStatSettingsMapper : ParentSettingsMapper<DerivedStatSettings, DerivedStat, DerivedStatSettingsApi> { }
}
