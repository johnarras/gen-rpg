using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Stats.Settings.Scaling
{
    [MessagePackObject]
    public class ScalingTypeSettings : ParentSettings<ScalingType>
    {
        [Key(0)] public override string Id { get; set; }

        public ScalingType GetScalingType(long idkey) { return _lookup.Get<ScalingType>(idkey); }
    }

    [MessagePackObject]
    public class ScalingTypeSettingsApi : ParentSettingsApi<ScalingTypeSettings, ScalingType> { }
    [MessagePackObject]
    public class ScalingTypeSettingsLoader : ParentSettingsLoader<ScalingTypeSettings, ScalingType, ScalingTypeSettingsApi> { }

}
