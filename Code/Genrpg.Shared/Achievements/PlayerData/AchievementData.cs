using MessagePack;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;

namespace Genrpg.Shared.Achievements.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class AchievementData : OwnerIdObjectList<AchievementStatus>
    {
        [Key(0)] public override string Id { get; set; }

        public long GetQuantity(long AchievementTypeId)
        {
            return Get(AchievementTypeId).Quantity;
        }

    }
    [MessagePackObject]
    public class AchievementStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long Quantity { get; set; }

    }

    [MessagePackObject]
    public class AchievementApi : OwnerApiList<AchievementData, AchievementStatus> { }
    [MessagePackObject]
    public class AchievementDataLoader : OwnerIdDataLoader<AchievementData, AchievementStatus> { }


    [MessagePackObject]
    public class AchievementDataMapper : OwnerDataMapper<AchievementData, AchievementStatus, AchievementApi> { }
}
