using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.PlayerData;
using System;
using Genrpg.Shared.Characters.PlayerData;
using System.Collections.Generic;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.Activities.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ActivityData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<ActivityStatus> Activities { get; set; } = new List<ActivityStatus>();
    }

    [MessagePackObject]
    public class ActivityStatus : IId
    {
        [Key(0)] public long IdKey { get; set; }
        [Key(1)] public int Tier { get; set; }
        [Key(2)] public int MaxTier { get; set; }
        [Key(3)] public long CurrPoints { get; set; }
        [Key(4)] public long MaxPoints { get; set; }
        [Key(5)] public long RewardEntityTypeId { get; set; }
        [Key(6)] public long RewardEntityId { get; set; }
        [Key(7)] public long RewardQuantity { get; set; }
        [Key(8)] public DateTime EndTime { get; set; }
    }

    [MessagePackObject]
    public class ActivityLoader : UnitDataLoader<ActivityData> { }

    [MessagePackObject]
    public class ActivityMapper : UnitDataMapper<ActivityData> { }
}
