using Genrpg.Shared.Core.Settings;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;
using MessagePack;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class BoardGenSettings : NoChildSettings // No List
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long SidePathMinLength { get; set; } = 20;
        [Key(2)] public long SidePathMaxLength { get; set; } = 30;

    }

    [MessagePackObject]
    public class BoardGenSettingsLoader : NoChildSettingsLoader<BoardGenSettings> { }

    [MessagePackObject]
    public class BoardGenSettingsMapper : NoChildSettingsMapper<BoardGenSettings> { }
}
