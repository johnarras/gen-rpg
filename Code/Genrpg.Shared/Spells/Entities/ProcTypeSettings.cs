using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class ProcTypeSettings : ParentSettings<ProcType>
    {
        [Key(0)] public override string Id { get; set; }

        public ProcType GetProcType(long idkey) { return _lookup.Get<ProcType>(idkey); }
    }

    [MessagePackObject]
    public class ProcTypeSettingsApi : ParentSettingsApi<ProcTypeSettings, ProcType> { }
    [MessagePackObject]
    public class ProcTypeSettingsLoader : ParentSettingsLoader<ProcTypeSettings, ProcType, ProcTypeSettingsApi> { }


}
