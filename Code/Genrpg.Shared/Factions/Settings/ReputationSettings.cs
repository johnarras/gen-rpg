using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings.Mappers;

namespace Genrpg.Shared.Factions.Settings
{
    [MessagePackObject]
    public class ReputationSettings : ParentSettings<RepLevel>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class RepLevel : ChildSettings, IIndexedGameItem
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string Desc { get; set; }
        [Key(5)] public string Icon { get; set; }

        [Key(6)] public string Art { get; set; }


        [Key(7)] public int PointsNeeded { get; set; }

    }

    [MessagePackObject]
    public class ReputationSettingsApi : ParentSettingsApi<ReputationSettings, RepLevel> { }

    [MessagePackObject]
    public class ReputationSettingsLoader : ParentSettingsLoader<ReputationSettings,RepLevel> { }

    [MessagePackObject]
    public class ReputationSettingsMapper : ParentSettingsMapper<ReputationSettings, RepLevel, ReputationSettingsApi> { }

}
