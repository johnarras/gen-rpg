using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Sexes.Settings
{
    /// <summary>
    /// List of equipment slots for characters
    /// </summary>
    [MessagePackObject]
    public class SexType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public long Armor { get; set; }
        [Key(8)] public long Damage { get; set; }


        [Key(9)] public long CostPercent { get; set; } = 100;

    }

    [MessagePackObject]
    public class SexTypeSettings : ParentSettings<SexType>
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public double LevelsPerQuality { get; set; } = 5.0f;

        [Key(2)] public double ExtraQualityChance { get; set; } = 0.25f;
    }

    [MessagePackObject]
    public class SexTypeSettingsApi : ParentSettingsApi<SexTypeSettings, SexType> { }
    [MessagePackObject]
    public class SexTypeSettingsLoader : ParentSettingsLoader<SexTypeSettings, SexType, SexTypeSettingsApi> { }

}
