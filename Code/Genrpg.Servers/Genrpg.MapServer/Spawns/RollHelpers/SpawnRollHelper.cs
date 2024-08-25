using Genrpg.MapServer.Spawns.Services;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Spawns.Settings;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spawns.RollHelpers
{
    public class SpawnRollHelper : IRollHelper
    {
        public long GetKey() { return EntityTypes.Spawn; }

        private ISpawnService _spawnService = null;
        private IGameData _gameData = null;
        public List<Reward> Roll<SI>(IRandom rand, RollData rollData, SI item) where SI : ISpawnItem
        {
            List<Reward> retval = new List<Reward>();
            long quantity = MathUtils.LongRange(item.MinQuantity, item.MaxQuantity, rand);

            SpawnTable st = _gameData.Get<SpawnSettings>(null).Get(item.EntityId);
            if (st != null)
            {
                for (int j = 0; j < quantity; j++)
                {
                    rollData.Depth++;
                    List<Reward> list2 = _spawnService.Roll(rand, st.Items, rollData);
                    rollData.Depth--;
                    retval.AddRange(list2);
                }
            }
            return retval;
        }
    }
}
