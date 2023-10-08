using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class FenceTypeSettings : ParentSettings<FenceType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<FenceType> Data { get; set; } = new List<FenceType>();

        public FenceType GetFenceType(long idkey) { return _lookup.Get<FenceType>(idkey); }
    }

    [MessagePackObject]
    public class FenceTypeSettingsApi : ParentSettingsApi<FenceTypeSettings, FenceType> { }
    [MessagePackObject]
    public class FenceTypeSettingsLoader : ParentSettingsLoader<FenceTypeSettings, FenceType, FenceTypeSettingsApi> { }


}
