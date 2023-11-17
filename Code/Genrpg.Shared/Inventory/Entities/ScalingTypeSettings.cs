using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Stats.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Entities
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
