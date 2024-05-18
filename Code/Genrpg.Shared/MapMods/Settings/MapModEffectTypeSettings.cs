using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.MapMods.Settings
{
    [MessagePackObject]
    public class MapModEffectType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }
    }
    [MessagePackObject]
    public class MapModEffectTypeSettings : ParentSettings<MapModEffectType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class MapModEffectTypeSettingsApi : ParentSettingsApi<MapModEffectTypeSettings, MapModEffectType> { }
    [MessagePackObject]
    public class MapModEffectSettingsLoader : ParentSettingsLoader<MapModEffectTypeSettings, MapModEffectType> { }

    [MessagePackObject]
    public class MapModEffectSettingsMapper : ParentSettingsMapper<MapModEffectTypeSettings, MapModEffectType, MapModEffectTypeSettingsApi> { }

}
