using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.SpellCrafting.Settings
{
    [MessagePackObject]
    public class SpellModifier : ChildSettings, IIndexedGameItem
    {
        public const int DefaultCostScale = 100;

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }

        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Icon { get; set; }
        [Key(5)] public string Art { get; set; }
        [Key(6)] public string Desc { get; set; }
        [Key(7)] public string DisplaySuffix { get; set; }
        [Key(8)] public string DataMemberName { get; set; }
        [Key(9)] public bool IsProcMod { get; set; }
        [Key(10)] public float DisplayMult { get; set; }

        [Key(11)] public double MinValue { get; set; }
        [Key(12)] public double MaxValue { get; set; }
        [Key(13)] public double ValueDelta { get; set; }
        [Key(14)] public double DefaultValue { get; set; }

        public SpellModifier()
        {
        }
    }
    [MessagePackObject]
    public class SpellModifierSettings : ParentSettings<SpellModifier>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class SpellModifierSettingsApi : ParentSettingsApi<SpellModifierSettings, SpellModifier> { }
    [MessagePackObject]
    public class SpellModifierSettingsLoader : ParentSettingsLoader<SpellModifierSettings, SpellModifier> { }

    [MessagePackObject]
    public class SpellModifierSettingsMapper : ParentSettingsMapper<SpellModifierSettings, SpellModifier, SpellModifierSettingsApi> { }
}
