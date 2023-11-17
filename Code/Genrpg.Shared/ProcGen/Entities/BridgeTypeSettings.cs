using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class BridgeTypeSettings : ParentSettings<BridgeType>
    {
        [Key(0)] public override string Id { get; set; }

        public BridgeType GetBridgeType(long idkey) { return _lookup.Get<BridgeType>(idkey); }
    }

    [MessagePackObject]
    public class BridgeTypeSettingsApi : ParentSettingsApi<BridgeTypeSettings, BridgeType> { }
    [MessagePackObject]
    public class BridgeTypeSettingsLoader : ParentSettingsLoader<BridgeTypeSettings, BridgeType, BridgeTypeSettingsApi> { }


}
