using MessagePack;
using System;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;

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


    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class UserCoinData : OwnerIdObjectList<UserCoinStatus>
    {
        [Key(0)] public override string Id { get; set; }
        public long GetQuantity(long userCoinTypeId)
        {
            return Get(userCoinTypeId).Quantity;
        }

        public bool Add(long userCoinTypeId, long quantity)
        {
            return Set(userCoinTypeId, GetQuantity(userCoinTypeId) + quantity);
        }

        public bool Set(long userCoinTypeId, long newQuantity)
        {
            if (newQuantity < 0)
            {
                newQuantity = 0;
            }

            UserCoinStatus status = Get(userCoinTypeId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = newQuantity;
            return true;
        }
    }

    [MessagePackObject]
    public class UserCoinDataLoader : OwnerIdDataLoader<UserCoinData, UserCoinStatus>
    {
        protected override bool IsUserData() { return true; }
    }
    [MessagePackObject]
    public class UserCoinApi : OwnerApiList<UserCoinData, UserCoinStatus> { }

    [MessagePackObject]
    public class UserCoinDataMapper : OwnerDataMapper<UserCoinData,UserCoinStatus, UserCoinApi> { }
}
