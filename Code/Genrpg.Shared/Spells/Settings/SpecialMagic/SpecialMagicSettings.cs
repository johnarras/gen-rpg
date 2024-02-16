using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.SpecialMagic
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
    public class SpecialMagic : ChildSettings, IIndexedGameItem
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
    public class SpecialMagicSettings : ParentSettings<SpecialMagic>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class SpecialMagicSettingsApi : ParentSettingsApi<SpecialMagicSettings, SpecialMagic> { }
    [MessagePackObject]
    public class SpecialMagicSettingsLoader : ParentSettingsLoader<SpecialMagicSettings, SpecialMagic, SpecialMagicSettingsApi> { }
}
