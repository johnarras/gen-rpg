
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Services;
using System.Threading;
using System.Runtime.InteropServices;
using Genrpg.Shared.Spawns.Interfaces;
using System.Linq;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Rewards.Entities;

namespace Genrpg.MapServer.Spawns.Services
{

    public interface ISpawnService : IInitializable
    {
        List<RewardList> Roll(IRandom rand, long spawnTableId, RollData rollData);
        List<RewardList> Roll(IRandom rand, SpawnTable st, RollData rollData);
        List<RewardList> Roll<SI>(IRandom rand, List<SI> items, RollData rollData) where SI : ISpawnItem;
    }

    /// <summary>
    /// This class is used to roll treasure and other items. 
    /// It's set up so that it's possible to say have a generic
    /// humanoid monster SpawnTable that gives level appropriate loot,
    /// and then for specific monsters to create a new parent spawn table
    /// that always rolls this generic table once, and then adds some extra 
    /// loot. 
    /// </summary>
    public class SpawnService : ISpawnService
    {
        private IGameData _gameData = null;
        private SetupDictionaryContainer<long, IRollHelper> _rollHelpers = new();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        protected IRollHelper GetRollHelper(long entityTypeId)
        {
            if (_rollHelpers.TryGetValue(entityTypeId, out IRollHelper helper))
            {
                return helper;
            }
            return null;
        }

        public List<RewardList> Roll(IRandom rand, long spawnTableId, RollData rollData)
        {
            return Roll(rand, _gameData.Get<SpawnSettings>(null).Get(spawnTableId), rollData);
        }

        // Different public roll methods.

        public List<RewardList> Roll(IRandom rand, SpawnTable st, RollData rollData)
        {
            if (st == null)
            {
                return new List<RewardList>();
            }

            return Roll(rand, st.Items, rollData);
        }

        public List<RewardList> Roll<SI>(IRandom rand, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            return InnerRoll(rand, items, rollData);
        }

        private List<RewardList> InnerRoll<SI>(IRandom rand, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            List<RewardList> list = new List<RewardList>();

            list.Add(new RewardList() { RewardSourceId = rollData.RewardSourceId, EntityId = rollData.EntityId });
            for (int i = 0; i < rollData.Times; i++)
            {
                rollData.Depth++;
                list.AddRange(RollOnce(rand, items, rollData));
                rollData.Depth--;
            }

            return list;
        }


        /// <summary>
        /// Roll against the spawn table once.
        /// </summary>
        /// <param name="gs">GameState</param>
        /// <param name="st">Spawn Table to roll on</param>
        /// <param name="level">Level of loot</param>
        /// <param name="qualityTypeId">Power of the loot</param>
        /// <param name="depth">Depth of the recursion</param>
        /// <returns>A list of spawn results</returns>
        private List<RewardList> RollOnce<SI>(IRandom rand, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            if (items == null)
            {
                return new List<RewardList>();
            }

            List<RewardList> retval = new List<RewardList>();
            Dictionary<int, List<SI>> groupDict = new Dictionary<int, List<SI>>();
            List<SI> rollEachList = new List<SI>();
            foreach (SI si in items)
            {
                if (si.MinLevel > rollData.Level)
                {
                    continue;
                }
                if (si.GroupId < 1)
                {
                    if (rand.NextDouble() * 100 < si.Weight)
                    {
                        rollData.Depth++;
                        retval = retval.Concat(RollOneItem(rand, si, rollData)).ToList();
                        rollData.Depth--;
                    }
                    continue;
                }
                if (!groupDict.ContainsKey(si.GroupId))
                {
                    groupDict[si.GroupId] = new List<SI>();
                }
                groupDict[si.GroupId].Add(si);

            }

            foreach (int key in groupDict.Keys)
            {
                double totalRollWeight = 0;
                List<SI> groupList = groupDict[key];
                foreach (SI si in groupList)
                {
                    totalRollWeight += si.Weight;
                }

                if (totalRollWeight <= 0)
                {
                    continue;
                }

                double weightChosen = rand.NextDouble() * totalRollWeight;

                foreach (SI si in groupList)
                {
                    weightChosen -= si.Weight;
                    if (weightChosen <= 0)
                    {
                        rollData.Depth++;
                        retval = retval.Concat(RollOneItem(rand, si, rollData)).ToList();
                        rollData.Depth--;
                        break;
                    }
                }
            }
            return retval;
        }


        private List<RewardList> RollOneItem<SI>(IRandom rand, SI si, RollData rollData) where SI : ISpawnItem
        {
            List<RewardList> retval = new List<RewardList>();

            if (rollData.Depth > 10)
            {
                return retval;
            }

            IRollHelper rollHelper = GetRollHelper(si.EntityTypeId);

            if (rollHelper != null)
            {
                retval = rollHelper.Roll(rand, rollData, si);
                return retval;
            }


            RewardList rewardList = new RewardList();
            retval.Add(rewardList);
            long quantity = MathUtils.LongRange(si.MinQuantity, si.MaxQuantity, rand);

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            rew.QualityTypeId = rollData.QualityTypeId;
            rew.Level = rollData.Level;
            rewardList.Rewards.Add(rew);

            return retval;
        }


    }
}
