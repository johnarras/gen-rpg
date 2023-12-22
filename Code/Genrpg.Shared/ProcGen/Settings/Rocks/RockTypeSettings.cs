using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Rocks
{
    [MessagePackObject]
    public class RockTypeSettings : ParentSettings<RockType>
    {
        [Key(0)] public override string Id { get; set; }

        public RockType GetRockType(long idkey) { return _lookup.Get<RockType>(idkey); }
    }

    [MessagePackObject]
    public class RockTypeSettingsApi : ParentSettingsApi<RockTypeSettings, RockType> { }
    [MessagePackObject]
    public class RockTypeSettingsLoader : ParentSettingsLoader<RockTypeSettings, RockType, RockTypeSettingsApi> { }

}
