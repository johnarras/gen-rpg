using MessagePack;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData;

namespace Genrpg.Shared.UserCoins.PlayerData
{
    [MessagePackObject]
    public class UserCoinStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public long Quantity { get; set; }
    }


}
