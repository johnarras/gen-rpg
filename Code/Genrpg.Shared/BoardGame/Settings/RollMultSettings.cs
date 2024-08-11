using MessagePack;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.BoardGame.Settings
{
    [MessagePackObject]
    public class RollMultSettings : ParentSettings<RollMult>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class RollMult : ChildSettings, IIdName
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public long Mult { get; set; }
        [Key(5)] public long MinLevel { get; set; }
        [Key(6)] public long MinDice { get; set; }
        [Key(7)] public double AvgRollMultChangePercent { get; set; }
    }

    [MessagePackObject]
    public class RollMultSettingsApi : ParentSettingsApi<RollMultSettings, RollMult> { }

    [MessagePackObject]
    public class RollMultSettingsLoader : ParentSettingsLoader<RollMultSettings, RollMult> { }

    [MessagePackObject]
    public class RollMultSettingsMapper : ParentSettingsMapper<RollMultSettings, RollMult, RollMultSettingsApi> { }
}
