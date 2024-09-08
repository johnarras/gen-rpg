using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
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

        public bool GiveReward(IRandom rand, MapObject obj, long entityId, long quantity, object extraData = null)
        {
            obj.Get<TParent>().Get(entityId).Quantity += quantity;
            return true;
        }

        public bool Add(MapObject obj, long entityId, long quantity)
        {
            return Set(obj, entityId, Get(obj, entityId) + quantity);
        }

        public long Get(MapObject obj, long entityId)
        {
            return obj.Get<TParent>().Get(entityId).Quantity;
        }

        public abstract long GetKey();

        public bool Set(MapObject obj, long entityId, long quantity)
        {
            TParent parentData = obj.Get<TParent>();
            TChild status = parentData.Get(entityId);
            long oldQuantity = Math.Max(0, status.Quantity);
            status.Quantity = quantity;

            long diff = quantity - oldQuantity;

            if (diff != 0)
            {
                _rewardService.OnSetQuantity(obj, status, GetKey(), diff);
            }
            return true;
        }
    }
}
