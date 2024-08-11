using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using System;
using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Chests.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ChestData : OwnerObjectList<ChestStatus>
    {
        [Key(0)] public override string Id { get; set; }

    }

    [MessagePackObject]
    public class ChestStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public int Slot { get; set; }
        [Key(4)] public DateTime UnlockTime { get; set; } = DateTime.MinValue;

    }

    [MessagePackObject]
    public class ChestApi : OwnerApiList<ChestData, ChestStatus> { }

    [MessagePackObject]
    public class ChestDataLoader : OwnerIdDataLoader<ChestData, ChestStatus> { }


    [MessagePackObject]
    public class ChestDataMapper : OwnerDataMapper<ChestData, ChestStatus, ChestApi> { }
}
