using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class ClutterTypeSettings : ParentSettings<ClutterType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<ClutterType> Data { get; set; } = new List<ClutterType>();

        public ClutterType GetClutterType(long idkey) { return _lookup.Get<ClutterType>(idkey); }
    }

    [MessagePackObject]
    public class ClutterTypeSettingsApi : ParentSettingsApi<ClutterTypeSettings, ClutterType> { }
    [MessagePackObject]
    public class ClutterTypeSettingsLoader : ParentSettingsLoader<ClutterTypeSettings, ClutterType, ClutterTypeSettingsApi> { }


}
