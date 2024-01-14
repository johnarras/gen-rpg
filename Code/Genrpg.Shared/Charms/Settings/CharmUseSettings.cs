using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Interfaces;
using MessagePack;

namespace Genrpg.Shared.Charms.Settings
{
    [MessagePackObject]
    public class CharmUse : ChildSettings, IIdName
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
    }
    [MessagePackObject]
    public class CharmUseSettings : ParentSettings<CharmUse>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class CharmUseSettingsApi : ParentSettingsApi<CharmUseSettings, CharmUse> { }
    [MessagePackObject]
    public class CharmUseSettingsLoader : ParentSettingsLoader<CharmUseSettings, CharmUse, CharmUseSettingsApi> { }
}
