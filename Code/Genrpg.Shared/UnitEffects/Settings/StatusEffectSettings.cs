using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Purchasing.Settings;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.UnitEffects.Settings
{

    public enum EActionEffects
    {
        None = 0,
        Weaken=1,
        Set=2,
        Block=3,
        Strengthen = 4,
    };


    [MessagePackObject]
    public class ActionEffect
    {
        [Key(0)] public long CombatActionId { get; set; }
        [Key(1)] public EActionEffects Effect { get; set; } = EActionEffects.None;
    }
        

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
        [Key(8)] public List<ActionEffect> ActionEffects { get; set; } = new List<ActionEffect>();
    }

    [MessagePackObject]
    public class StatusEffectSettings : ParentSettings<StatusEffect>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class StatusEffectSettingsApi : ParentSettingsApi<StatusEffectSettings, StatusEffect> { }
    [MessagePackObject]
    public class StatusEffectSettingsLoader : ParentSettingsLoader<StatusEffectSettings, StatusEffect> { }

    [MessagePackObject]
    public class StatusEffectSettingsMapper : ParentSettingsMapper<StatusEffectSettings, StatusEffect, StatusEffectSettingsApi> { }

}
