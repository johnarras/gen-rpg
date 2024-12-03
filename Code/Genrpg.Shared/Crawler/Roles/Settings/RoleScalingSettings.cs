using MessagePack;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Mappers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Linq;

namespace Genrpg.Shared.Crawler.Roles.Settings
{
    [MessagePackObject]
    public class RoleScalingTypeSettings : ParentSettings<RoleScalingType>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public long PointsPerLevel { get; set; }

    }

    [MessagePackObject]
    public class RoleScalingType : ChildSettings, IIndexedGameItem
    {

        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string ParentId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public override string Name { get; set; }
        [Key(4)] public string NameId { get; set; }
        [Key(5)] public string Desc { get; set; }
        [Key(6)] public string Icon { get; set; }
        [Key(7)] public string Art { get; set; }
        [Key(8)] public double MaxBonus { get; set; }
        [Key(9)] public double BonusPerLevel { get; set; }


    }


    [MessagePackObject]
    public class RoleScalingTypeSettingsApi : ParentSettingsApi<RoleScalingTypeSettings, RoleScalingType> { }
    [MessagePackObject]
    public class RoleScalingTypeSettingsLoader : ParentSettingsLoader<RoleScalingTypeSettings, RoleScalingType> { }

    [MessagePackObject]
    public class RoleScalingTypeSettingsMapper : ParentSettingsMapper<RoleScalingTypeSettings, RoleScalingType, RoleScalingTypeSettingsApi> { }
}
