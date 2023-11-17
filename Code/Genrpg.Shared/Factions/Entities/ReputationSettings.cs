using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.GameSettings;
using Genrpg.Shared.GameSettings.Loaders;

namespace Genrpg.Shared.Factions.Entities
{
    [MessagePackObject]
    public class ReputationSettings : ParentSettings<RepLevel>
    {
        [Key(0)] public override string Id { get; set; }

        public RepLevel GetRepLevel(long idkey) { return _lookup.Get<RepLevel>(idkey); }
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
    public class ReputationSettingsLoader : GameSettingsLoader<ReputationSettings> { }
}
