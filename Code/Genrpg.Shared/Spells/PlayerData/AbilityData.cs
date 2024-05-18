using MessagePack;

using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Utils;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;

namespace Genrpg.Shared.Spells.PlayerData
{
    [MessagePackObject]
    public class AbilityRank : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long AbilityCategoryId { get; set; }
        [Key(3)] public long AbilityTypeId { get; set; }
        [Key(4)] public int Rank { get; set; }
    }
    [MessagePackObject]
    public class AbilityData : OwnerObjectList<AbilityRank>
    {
        [Key(0)] public override string Id { get; set; }

    }

    [MessagePackObject]
    public class AbilityDataLoader : OwnerDataLoader<AbilityData, AbilityRank> { }

    [MessagePackObject]
    public class AbilityApi : OwnerApiList<AbilityData, AbilityRank> { }

    [MessagePackObject]
    public class AbiliyDataMapper : OwnerDataMapper<AbilityData, AbilityRank, AbilityApi> { }

}
