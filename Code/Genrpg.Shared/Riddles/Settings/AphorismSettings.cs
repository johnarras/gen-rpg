using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Purchasing.Settings;

namespace Genrpg.Shared.Aphorisms.Settings
{

    [MessagePackObject]
    public class AphorismSettings : ParentSettings<Aphorism>
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class Aphorism : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
    }

    [MessagePackObject]
    public class AphorismSettingsApi : ParentSettingsApi<AphorismSettings, Aphorism> { }
    [MessagePackObject]
    public class AphorismSettingsLoader : ParentSettingsLoader<AphorismSettings, Aphorism> { }

    [MessagePackObject]
    public class AphorismSettingsMapper : ParentSettingsMapper<AphorismSettings, Aphorism, AphorismSettingsApi> { }

}