using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.UnitEffects.Settings
{
    [MessagePackObject]
    public class StatusEffect : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }
        [Key(6)] public string Art { get; set; }
        [Key(7)] public string Abbrev { get; set; }

    }

    [MessagePackObject]
    public class StatusEffectSettings : ParentSettings<StatusEffect>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StatusEffectSettingsApi : ParentSettingsApi<StatusEffectSettings, StatusEffect> { }
    [MessagePackObject]
    public class StatusEffectSettingsLoader : ParentSettingsLoader<StatusEffectSettings, StatusEffect, StatusEffectSettingsApi> { }

}
