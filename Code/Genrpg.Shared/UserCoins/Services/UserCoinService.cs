using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.UserCoins.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Genrpg.Shared.UserCoins.Services
{
    public class UserCoinService : IUserCoinService
    {
        public bool Add(Unit unit, long userCoinTypeId, long quantity)
        {
            return Set(unit, userCoinTypeId, unit.Get<UserCoinData>().GetQuantity(userCoinTypeId) + quantity);
        }

        public bool Set(Unit unit, long userCoinTypeId, long newQuantity)
        {
            if (newQuantity < 0)
            {
                newQuantity = 0;
            }
            UserCoinData userCoinData = unit.Get<UserCoinData>();
            UserCoinStatus status = userCoinData.Get(userCoinTypeId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = newQuantity;

            long diff = newQuantity - oldQuantity;

            OnSetUserCoin(unit, userCoinData, status, diff);
            return true;
        }

        protected virtual void OnSetUserCoin(Unit unit, UserCoinData userCoinData, UserCoinStatus status, long diff)
        {

        }
    }
}
