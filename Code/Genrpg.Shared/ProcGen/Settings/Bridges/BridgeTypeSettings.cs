using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Settings.Bridges
{
    [MessagePackObject]
    public class BridgeType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public int Length { get; set; }

        public BridgeType()
        {
            Art = "Bridge";
            Length = 6;
        }

    }
    [MessagePackObject]
    public class BridgeTypeSettings : ParentSettings<BridgeType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class BridgeTypeSettingsApi : ParentSettingsApi<BridgeTypeSettings, BridgeType> { }
    [MessagePackObject]
    public class BridgeTypeSettingsLoader : ParentSettingsLoader<BridgeTypeSettings, BridgeType> { }

    [MessagePackObject]
    public class BridgeSettingsMapper : ParentSettingsMapper<BridgeTypeSettings, BridgeType, BridgeTypeSettingsApi> { }


}
