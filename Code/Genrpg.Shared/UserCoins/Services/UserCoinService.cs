using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.UserCoins.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UserCoins.Services
{
    public class UserCoinService : IUserCoinService
    {
        public bool Add(GameState gs, Unit unit, long userCoinTypeId, long quantity)
        {
            return Set(gs, unit, userCoinTypeId, unit.Get<UserCoinData>().GetQuantity(userCoinTypeId) + quantity);
        }

        public bool Set(GameState gs, Unit unit, long userCoinTypeId, long newQuantity)
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

            OnSetUserCoin(gs, unit, userCoinData, status, diff);
            return true;
        }

        protected virtual void OnSetUserCoin(GameState gs, Unit unit, UserCoinData userCoinData, UserCoinStatus status, long diff)
        {

        }
    }
}
