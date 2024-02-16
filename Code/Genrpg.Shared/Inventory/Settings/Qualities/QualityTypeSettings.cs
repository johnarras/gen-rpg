using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Inventory.Settings.Qualities
{
    [MessagePackObject]
    public class QualityType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        // Scaling for generating items.
        [Key(7)] public int ItemSpawnWeight { get; set; }
        [Key(8)] public int ItemMinLevel { get; set; }
        [Key(9)] public int ItemStatPct { get; set; }
        [Key(10)] public int ItemCostPct { get; set; }

        // Scaling for generating monsters.
        [Key(11)] public string UnitName { get; set; }
        [Key(12)] public int UnitSpawnWeight { get; set; }
        [Key(13)] public int UnitMinLevel { get; set; }
        [Key(14)] public int UnitHealthPct { get; set; }
        [Key(15)] public int UnitDamPct { get; set; }
    }

    [MessagePackObject]
    public class QualityName
    {
        [Key(0)] public long QualityTypeId { get; set; }
        [Key(1)] public string Name { get; set; }
    }

    [MessagePackObject]
    public class QualityTypeSettings : ParentSettings<QualityType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class QualityTypeSettingsApi : ParentSettingsApi<QualityTypeSettings, QualityType> { }
    [MessagePackObject]
    public class QualityTypeSettingsLoader : ParentSettingsLoader<QualityTypeSettings, QualityType, QualityTypeSettingsApi> { }

}
