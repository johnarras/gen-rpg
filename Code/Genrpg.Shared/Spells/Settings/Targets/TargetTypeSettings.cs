using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.Targets
{
    /// <summary>
    /// What kind of target a spell has.
    /// 
    /// When crafting spells, Buffs can only be added to other buffs.
    /// But spells with Ally+Enemy parts can both be combined. (like damage+heal)
    /// 
    /// 
    /// </summary>
    [MessagePackObject]
    public class TargetType : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }

        [Key(7)] public double StatBonusScale { get; set; }
    }
    [MessagePackObject]
    public class TargetTypeSettings : ParentSettings<TargetType>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class TargetTypeSettingsApi : ParentSettingsApi<TargetTypeSettings, TargetType> { }
    [MessagePackObject]
    public class TargetTypeSettingsLoader : ParentSettingsLoader<TargetTypeSettings, TargetType> { }

    [MessagePackObject]
    public class TargetTypeSettingsMapper : ParentSettingsMapper<TargetTypeSettings, TargetType, TargetTypeSettingsApi> { }
}
