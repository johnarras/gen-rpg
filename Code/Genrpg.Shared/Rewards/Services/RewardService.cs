using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Rewards.Services
{
    public class RewardService : IRewardService
    {

        protected IRepositoryService _repoService;

        private SetupDictionaryContainer<long, IRewardHelper> _rewardHelpers = new SetupDictionaryContainer<long, IRewardHelper>();
        protected IRewardHelper GetRewardHelper(long entityTypeId)
        {
            if (_rewardHelpers.TryGetValue(entityTypeId, out IRewardHelper helper))
            {
                return helper;
            }
            return null;
        }

        public virtual bool GiveRewards<RL>(IRandom rand, MapObject obj, List<RL> resultList) where RL : RewardList            
        {
            if (resultList == null)
            {
                return false;
            }
            bool hadFailure = false;
            if (obj is Character ch)
            {
                foreach (RewardList rl in resultList)
                {
                    foreach (Reward reward in rl.Rewards)
                    {
                        if (!GiveReward(rand, ch, reward))
                        {
                            hadFailure = true;
                        }
                    }
                }
            }
            else
            {
                hadFailure = true;
            }

            return !hadFailure;
        }

        public virtual bool GiveReward(IRandom rand, MapObject obj, Reward res)
        {
            return GiveReward(rand, obj, res.EntityTypeId, res.EntityId, res.Quantity, res.ExtraData);
        }

        public virtual bool GiveReward(IRandom rand, MapObject obj, long entityType, long entityId, long quantity, object extraData = null)
        {
            IRewardHelper helper = GetRewardHelper(entityType);

            if (helper == null)
            {
                return false;
            }

            // This handles any extra results we need to send to the client.
            return helper.GiveReward(rand, obj, entityId, quantity, extraData);
        }

        public bool Add(MapObject obj, long entityTypeId, long entityId, long quantity)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Add(obj, entityId, quantity);
            }
            return false;
        }

        public bool Set(MapObject obj, long entityTypeId, long entityId, long quantity)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Set(obj, entityId, quantity);
            }
            return false;
        }

        public virtual void OnAddQuantity<TUpd>(MapObject obj, TUpd upd, long entityTypeId, long entityId, long diff) where TUpd: class, IStringId
        {
        }
    }
}
