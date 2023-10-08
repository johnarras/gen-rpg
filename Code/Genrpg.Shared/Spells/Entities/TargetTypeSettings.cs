using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Entities
{
    [MessagePackObject]
    public class TargetTypeSettings : ParentSettings<TargetType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<TargetType> Data { get; set; } = new List<TargetType>();

        public TargetType GetTargetType(long idkey) { return _lookup.Get<TargetType>(idkey); }
    }

    [MessagePackObject]
    public class TargetTypeSettingsApi : ParentSettingsApi<TargetTypeSettings, TargetType> { }
    [MessagePackObject]
    public class TargetTypeSettingsLoader : ParentSettingsLoader<TargetTypeSettings, TargetType, TargetTypeSettingsApi> { }
}
