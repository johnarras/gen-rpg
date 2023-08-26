using MessagePack;
using Genrpg.Shared.DataStores.Core;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Genrpg.Shared.Spells.Entities;

namespace Genrpg.Shared.UserCoins.Entities
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class UserCoinData : IdObjectList<UserCoinStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override List<UserCoinStatus> Data { get; set; } = new List<UserCoinStatus>();
        public override void AddTo(Unit unit) { unit.Set(this); }
        protected override bool CreateIfMissingOnGet()
        {
            return true;
        }
        public override void Delete(IRepositorySystem repoSystem) { repoSystem.Delete(this); }
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
