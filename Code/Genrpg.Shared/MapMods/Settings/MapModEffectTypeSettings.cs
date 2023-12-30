using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMods.Settings
{
    [MessagePackObject]
    public class MapModEffectTypeSettings : ParentSettings<MapModEffectType>
    {
        [Key(0)] public override string Id { get; set; }

        public MapModEffectType GetMapModEffect(long idkey) { return _lookup.Get<MapModEffectType>(idkey); }
    }

    [MessagePackObject]
    public class MapModEffectSettingsApi : ParentSettingsApi<MapModEffectTypeSettings, MapModEffectType> { }
    [MessagePackObject]
    public class MapModEffectSettingsLoader : ParentSettingsLoader<MapModEffectTypeSettings, MapModEffectType, MapModEffectSettingsApi> { }

}
