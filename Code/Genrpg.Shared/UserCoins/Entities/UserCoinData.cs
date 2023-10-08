using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.DataStores.PlayerData;

namespace Genrpg.Shared.UserCoins.Entities
{
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
}
