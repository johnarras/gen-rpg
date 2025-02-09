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


        [Key(3)] public int EdgeMinGap { get; set; } = 1;
        [Key(4)] public int EdgeMaxGap { get; set; } = 4;
        [Key(5)] public float SlopeMax { get; set; } = 1;
        [Key(6)] public int MinPointCount { get; set; } = 10;
        [Key(7)] public int MaxPointCount { get; set; } = 18;
        [Key(8)] public float RadDelta { get; set; } = 0.5f;
    }
    

    [MessagePackObject]
    public class BoardGenSettingsLoader : NoChildSettingsLoader<BoardGenSettings> { }

    [MessagePackObject]
    public class BoardGenSettingsMapper : NoChildSettingsMapper<BoardGenSettings> { }
}
