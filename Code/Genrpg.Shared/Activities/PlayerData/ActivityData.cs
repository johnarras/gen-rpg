using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Currencies.PlayerData;
using System;
using Genrpg.Shared.Characters.PlayerData;

namespace Genrpg.Shared.Activities.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ActivityData : OwnerIdObjectList<ActivityStatus>
    {
        [Key(0)] public override string Id { get; set; }
    }

    [MessagePackObject]
    public class ActivityStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long Level { get; set; }
        [Key(4)] public long MaxLevel { get; set; }
        [Key(5)] public long CurrPoints { get; set; }
        [Key(6)] public long MaxPoints { get; set; }
        [Key(7)] public long RewardEntityTypeId { get; set; }
        [Key(8)] public long RewardEntityId { get; set; }
        [Key(9)] public long RewardProperty { get; set; }
        [Key(10)] public DateTime StartTime { get; set; }
        [Key(11)] public DateTime EndTime { get; set; }

    }

    [MessagePackObject]
    public class ActivityApi : OwnerApiList<ActivityData, ActivityStatus> { }

    [MessagePackObject]
    public class ActivityDataLoader : OwnerIdDataLoader<ActivityData, ActivityStatus> { }



    [MessagePackObject]
    public class ActivityDataMapper : OwnerDataMapper<CurrencyData, CurrencyStatus, CurrencyApi> { }
}
