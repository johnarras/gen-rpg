
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Entities.Services
{
    public interface IEntityService : IInitializable
    {
        bool GiveRewards<SR>(IRandom rand, MapObject obj, List<SR> resultList) where SR : ISpawnResult;
        IEntityHelper GetEntityHelper(long entityTypeId);
        IIndexedGameItem Find(IFilteredObject obj, long entityType, long entityId);
    }

    public class EntityService : IEntityService
    {

        private Dictionary<long, IRewardHelper> _rewardHelpers = null;
        private Dictionary<long, IEntityHelper> _entityHelpers = null;

        public async Task Initialize(IGameState gs, CancellationToken token)
        {
            _rewardHelpers = ReflectionUtils.SetupDictionary<long, IRewardHelper>(gs);
            _entityHelpers = ReflectionUtils.SetupDictionary<long, IEntityHelper>(gs);
            await Task.CompletedTask;
        }

        #region Rewards
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

        protected bool GiveReward(IRandom rand, Character ch, ISpawnResult res)
        {
            if (res == null)
            {
                return false;
            }

            return GiveReward(rand, ch, res.EntityTypeId, res.EntityId, res.Quantity, res.Data);
        }


        protected virtual bool GiveReward(IRandom rand, Character ch, long entityType, long entityId, long quantity, object extraData = null)
        {
            IRewardHelper helper = GetRewardHelper(entityType);

            if (helper == null)
            {
                return false;
            }

            // This handles any extra results we need to send to the client.
            return helper.GiveReward(rand, ch, entityId, quantity, extraData);
        }
        #endregion

        #region Helpers
        protected IRewardHelper GetRewardHelper(long entityTypeId)
        {
            if (_rewardHelpers.ContainsKey(entityTypeId))
            {
                return _rewardHelpers[entityTypeId];
            }
            return null;
        }

        public IEntityHelper GetEntityHelper(long entityTypeId)
        {
            if (_entityHelpers.ContainsKey(entityTypeId))
            {
                return _entityHelpers[entityTypeId];
            }
            return null;
        }
        public IIndexedGameItem Find( IFilteredObject obj, long entityType, long entityId)
        {
            IEntityHelper helper = GetEntityHelper(entityType);

            if (helper == null)
            {
                return null;
            }

            return helper.Find(obj, entityId);

        }
        #endregion
    }
}
