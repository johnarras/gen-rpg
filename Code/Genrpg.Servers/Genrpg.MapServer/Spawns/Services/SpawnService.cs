
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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;

namespace Genrpg.MapServer.Spawns.Services
{

    public interface ISpawnService : IInitializable
    {
        List<SpawnResult> Roll(IRandom rand, long spawnTableId, RollData rollData);
        List<SpawnResult> Roll(IRandom rand, SpawnTable st, RollData rollData);
        List<SpawnResult> Roll(IRandom rand, List<SpawnItem> items, RollData rollData);
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
        private IGameData _gameData  = null;
        private SetupDictionaryContainer<long, IRollHelper> _rollHelpers = new();
        public async Task Initialize( CancellationToken token)
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

        public List<SpawnResult> Roll(IRandom rand, long spawnTableId, RollData rollData)
        {
            return Roll(rand, _gameData.Get<SpawnSettings>(null).Get(spawnTableId), rollData);
        }

        // Different public roll methods.

        public List<SpawnResult> Roll(IRandom rand, SpawnTable st, RollData rollData)
        {
            if (st == null)
            {
                return new List<SpawnResult>();
            }

            return Roll(rand, st.Items, rollData);
        }

        public List<SpawnResult> Roll(IRandom rand, List<SpawnItem> items, RollData rollData)
        {
            return InnerRoll(rand, items, rollData);
        }

        private List<SpawnResult> InnerRoll(IRandom rand, List<SpawnItem> items, RollData rollData)
        {
            List<SpawnResult> list = new List<SpawnResult>();

            for (int i = 0; i < rollData.Times; i++)
            {
                rollData.Depth++;
                list = list.Concat(RollOnce(rand, items, rollData)).ToList();
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
        private List<SpawnResult> RollOnce(IRandom rand, List<SpawnItem> items, RollData rollData)
        {
            if (items == null)
            {
                return new List<SpawnResult>();
            }

            List<SpawnResult> retval = new List<SpawnResult>();

            Dictionary<int, List<SpawnItem>> groupDict = new Dictionary<int, List<SpawnItem>>();
            List<SpawnItem> rollEachList = new List<SpawnItem>();
            foreach (SpawnItem si in items)
            {
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
                    groupDict[si.GroupId] = new List<SpawnItem>();
                }
                groupDict[si.GroupId].Add(si);

            }

            foreach (int key in groupDict.Keys)
            {
                double totalRollWeight = 0;
                List<SpawnItem> groupList = groupDict[key];
                foreach (SpawnItem si in groupList)
                {
                    totalRollWeight += si.Weight;
                }

                if (totalRollWeight <= 0)
                {
                    continue;
                }

                double weightChosen = rand.NextDouble() * totalRollWeight;

                foreach (SpawnItem si in groupList)
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


        private List<SpawnResult> RollOneItem(IRandom rand, SpawnItem si, RollData rollData)
        {
            List<SpawnResult> retval = new List<SpawnResult>();

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

            long quantity = MathUtils.LongRange(si.MinQuantity, si.MaxQuantity, rand);

            SpawnResult sr = new SpawnResult();
            sr.EntityId = si.EntityId;
            sr.EntityTypeId = si.EntityTypeId;
            sr.Quantity = quantity;
            sr.QualityTypeId = rollData.QualityTypeId;
            sr.Level = rollData.Level;
            retval.Add(sr);

            return retval;
        }


    }
}
