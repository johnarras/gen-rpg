using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loading;
using Genrpg.Shared.Zones.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.ProcGen.Entities
{
    [MessagePackObject]
    public class ZoneTypeSettings : ParentSettings<ZoneType>
    {
        [Key(0)] public override string Id { get; set; }

        public ZoneType GetZoneType(long idkey) { return _lookup.Get<ZoneType>(idkey); }
    }

    [MessagePackObject]
    public class ZoneTypeSettingsApi : ParentSettingsApi<ZoneTypeSettings, ZoneType> { }
    [MessagePackObject]
    public class ZoneTypeSettingsLoader : ParentSettingsLoader<ZoneTypeSettings, ZoneType, ZoneTypeSettingsApi> { }

}
