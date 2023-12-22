using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;

namespace Genrpg.Shared.Charms.Settings
{
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
