using Assets.Scripts.ClientEvents.UserCoins;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Rewards.Services
{
    public class ClientRewardService : RewardService
    {
        private IDispatcher _dispatcher;
        public override void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff)
        {
            if (entityTypeId == EntityTypes.UserCoin)
            {
                _dispatcher.Dispatch(new AddUserCoinVisual() { InstantUpdate = false, QuantityAdded = diff, UserCoinTypeId = entityId });
            }
        }
    }
}
