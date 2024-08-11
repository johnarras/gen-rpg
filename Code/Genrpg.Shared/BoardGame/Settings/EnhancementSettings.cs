using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class EnhancementSettings : ParentSettings<Enhancement>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class Enhancement : ChildSettings, IIndexedGameItem
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
    public class EnhancementSettingsApi : ParentSettingsApi<EnhancementSettings, Enhancement> { }

    [MessagePackObject]
    public class EnhancementSettingsLoader : ParentSettingsLoader<EnhancementSettings, Enhancement> { }

    [MessagePackObject]
    public class EnhancementSettingsMapper : ParentSettingsMapper<EnhancementSettings, Enhancement, EnhancementSettingsApi> { }
}
