using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Spawns.Entities;
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

        public virtual bool GiveRewards<SR>(IRandom rand, MapObject obj, List<SR> resultList) where SR : ISpawnResult
        {
            if (resultList == null)
            {
                return false;
            }
            bool hadFailure = false;
            if (obj is Character ch)
            {
                foreach (ISpawnResult res in resultList)
                {
                    if (!GiveReward(rand, ch, res))
                    {
                        hadFailure = true;
                    }
                }
            }
            else
            {
                hadFailure = true;
            }

            return !hadFailure;
        }

        public virtual bool GiveReward(IRandom rand, Unit unit, ISpawnResult res)
        {
            return GiveReward(rand, unit, res.EntityTypeId, res.EntityId, res.Quantity, res.Data);
        }

        public virtual bool GiveReward(IRandom rand, Unit unit, long entityType, long entityId, long quantity, object extraData = null)
        {
            IRewardHelper helper = GetRewardHelper(entityType);

            if (helper == null)
            {
                return false;
            }

            // This handles any extra results we need to send to the client.
            return helper.GiveReward(rand, unit, entityId, quantity, extraData);
        }

        public bool Add(Unit unit, long entityTypeId, long entityId, long quantity)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Add(unit, entityId, quantity);
            }
            return false;
        }

        public bool Set(Unit unit, long entityTypeId, long entityId, long quantity)
        {
            IQuantityRewardHelper quantityHelper = GetRewardHelper(entityTypeId) as IQuantityRewardHelper;
            if (quantityHelper != null)
            {
                return quantityHelper.Set(unit, entityId, quantity);
            }
            return false;
        }

        public virtual void OnSetQuantity<TUpd>(Unit unit, TUpd upd, long entityTypeId, long diff) where TUpd: class, IStringId
        {
        }
    }
}
