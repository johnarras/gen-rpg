using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.Procs
{
    [MessagePackObject]
    public class ProcType : ChildSettings, IIndexedGameItem
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
    public class ProcTypeSettings : ParentSettings<ProcType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ProcTypeSettingsApi : ParentSettingsApi<ProcTypeSettings, ProcType> { }
    [MessagePackObject]
    public class ProcTypeSettingsLoader : ParentSettingsLoader<ProcTypeSettings, ProcType> { }

    [MessagePackObject]
    public class ProcTypeSettingsMapper : ParentSettingsMapper<ProcTypeSettings, ProcType, ProcTypeSettingsApi> { }


}
