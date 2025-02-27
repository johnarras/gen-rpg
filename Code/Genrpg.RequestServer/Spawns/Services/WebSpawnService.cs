
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
using Genrpg.RequestServer.Spawns.Helpers;
using Genrpg.RequestServer.Core;

namespace Genrpg.RequestServer.Spawns.Services
{

    public interface IWebSpawnService : IInitializable
    {
        Task<List<RewardList>> Roll(WebContext context, long spawnTableId, RollData rollData);
        Task<List<RewardList>> Roll(WebContext context, SpawnTable st, RollData rollData);
        Task<List<RewardList>> Roll<SI>(WebContext context, List<SI> items, RollData rollData) where SI : ISpawnItem;
    }

    /// <summary>
    /// This class is used to roll treasure and other items. 
    /// It's set up so that it's possible to say have a generic
    /// humanoid monster SpawnTable that gives level appropriate loot,
    /// and then for specific monsters to create a new parent spawn table
    /// that always rolls this generic table once, and then adds some extra 
    /// loot. 
    /// </summary>
    public class WebSpawnService : IWebSpawnService
    {
        private IGameData _gameData = null;
        private SetupDictionaryContainer<long, IWebRollHelper> _rollHelpers = new();
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        private IWebRollHelper GetRollHelper(long entityTypeid)
        {
            if (_rollHelpers.TryGetValue(entityTypeid, out IWebRollHelper helper))
            {
                return helper;
            }
            return null;
        }

        public async Task<List<RewardList>> Roll(WebContext context, long spawnTableId, RollData rollData)
        {
            return await Roll(context, _gameData.Get<SpawnSettings>(null).Get(spawnTableId), rollData);
        }

        // Different public roll methods.

        public async Task<List<RewardList>> Roll(WebContext context, SpawnTable st, RollData rollData)
        {
            if (st == null)
            {
                return new List<RewardList>();
            }

            return await Roll(context, st.Items, rollData);
        }

        public async Task<List<RewardList>> Roll<SI>(WebContext context, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            return await InnerRoll(context, items, rollData);
        }

        private async Task<List<RewardList>> InnerRoll<SI>(WebContext context, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            List<RewardList> list = new List<RewardList>();

            list.Add(new RewardList() { RewardSourceId = rollData.RewardSourceId, EntityId = rollData.EntityId });
            for (int i = 0; i < rollData.Times; i++)
            {
                rollData.Depth++;
                list[0].Rewards = list[0].Rewards.Concat(await RollOnce(context, items, rollData)).ToList();
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
        /// <returns>A list of spawn responses</returns>
        private async Task<List<Reward>> RollOnce<SI>(WebContext context, List<SI> items, RollData rollData) where SI : ISpawnItem
        {
            if (items == null)
            {
                return new List<Reward>();
            }

            List<Reward> retval = new List<Reward>();

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
                    if (context.rand.NextDouble() * 100 < si.Weight)
                    {
                        rollData.Depth++;
                        retval = retval.Concat(await RollOneItem(context, si, rollData)).ToList();
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

                SI si = RandomUtils.GetRandomElement(groupDict[key], context.rand);

                if (si != null)
                {
                    rollData.Depth++;
                    retval = retval.Concat(await RollOneItem(context, si, rollData)).ToList();
                    rollData.Depth--;
                }
            }
            return retval;
        }


        private async Task<List<Reward>> RollOneItem<SI>(WebContext context, SI si, RollData rollData) where SI : ISpawnItem
        {
            List<Reward> retval = new List<Reward>();

            if (rollData.Depth > 10)
            {
                return retval;
            }

            IWebRollHelper rollHelper = GetRollHelper(si.EntityTypeId);

            if (rollHelper != null)
            {
                retval = await rollHelper.Roll(context, rollData, si);
                return retval;
            }

            long quantity = MathUtils.LongRange(si.MinQuantity, si.MaxQuantity, context.rand);

            Reward rew = new Reward();
            rew.EntityId = si.EntityId;
            rew.EntityTypeId = si.EntityTypeId;
            rew.Quantity = quantity;
            rew.QualityTypeId = rollData.QualityTypeId;
            rew.Level = rollData.Level;
            retval.Add(rew);

            return retval;
        }


    }
}
