using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using MessagePack;

namespace Genrpg.Shared.Charms.Settings
{
    [MessagePackObject]
    public class CharmBonusSettings : ParentSettings<CharmBonus>
    {
        [Key(0)] public override string Id { get; set; }
        public CharmBonus GetCharmBonus(long idkey) { return _lookup.Get<CharmBonus>(idkey); }
    }

    [MessagePackObject]
    public class CharmBonusSettingsApi : ParentSettingsApi<CharmBonusSettings, CharmBonus> { }
    [MessagePackObject]
    public class CharmBonusSettingsLoader : ParentSettingsLoader<CharmBonusSettings, CharmBonus, CharmBonusSettingsApi> { }
}
