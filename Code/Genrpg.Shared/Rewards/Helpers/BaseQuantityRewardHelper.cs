using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Rewards.Helpers
{
    public abstract class BaseQuantityRewardHelper<TParent, TChild> : IQuantityRewardHelper where TParent : OwnerQuantityObjectList<TChild>, new() where TChild : OwnerQuantityChild, IId, new()
    {

        protected IRewardService _rewardService;

        public bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null)
        {
            unit.Get<TParent>().Get(entityId).Quantity += quantity;
            return true;
        }

        public bool Add(Unit unit, long entityId, long quantity)
        {
            return Set(unit, entityId, Get(unit, entityId) + quantity);
        }

        public long Get(Unit unit, long entityId)
        {
            return unit.Get<TParent>().Get(entityId).Quantity;
        }

        public abstract long GetKey();

        public bool Set(Unit unit, long entityId, long quantity)
        {
            TParent parentData = unit.Get<TParent>();
            TChild status = parentData.Get(entityId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = quantity;

            long diff = quantity - oldQuantity;

            if (diff != 0)
            {
                _rewardService.OnSetQuantity(unit, status, GetKey(), diff);
            }
            return true;
        }
    }
}
