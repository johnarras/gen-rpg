using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class GroundObjTypeSettings : ParentSettings<GroundObjType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<GroundObjType> Data { get; set; } = new List<GroundObjType>();

        public GroundObjType GetGroundObjType(long idkey) { return _lookup.Get<GroundObjType>(idkey); }
    }

    [MessagePackObject]
    public class GroundObjTypeSettingsApi : ParentSettingsApi<GroundObjTypeSettings, GroundObjType> { }
    [MessagePackObject]
    public class GroundObjTypeSettingsLoader : ParentSettingsLoader<GroundObjTypeSettings, GroundObjType, GroundObjTypeSettingsApi> { }


}
