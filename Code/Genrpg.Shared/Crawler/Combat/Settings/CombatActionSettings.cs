using Genrpg.Shared.Crawler.Buffs.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Crawler.Combat.Settings
{
    [MessagePackObject]
    public class CombatAction : ChildSettings, IIndexedGameItem
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
    public class CombatActionSettings : ParentSettings<CombatAction>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CombatActionSettingsApi : ParentSettingsApi<CombatActionSettings, CombatAction> { }
    [MessagePackObject]
    public class CombatActionSettingsLoader : ParentSettingsLoader<CombatActionSettings, CombatAction> { }

    [MessagePackObject]
    public class CombatActionSettingsMapper : ParentSettingsMapper<CombatActionSettings, CombatAction, CombatActionSettingsApi> { }





}
