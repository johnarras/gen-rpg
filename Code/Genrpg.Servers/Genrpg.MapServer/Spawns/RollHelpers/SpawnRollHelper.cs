using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
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
        private IGameData _gameData;
        public List<SpawnResult> Roll(GameState gs, RollData rollData, SpawnItem item)
        {
            List<SpawnResult> retval = new List<SpawnResult>();
            long quantity = MathUtils.LongRange(item.MinQuantity, item.MaxQuantity, gs.rand);

            SpawnTable st = _gameData.Get<SpawnSettings>(null).Get(item.EntityId);
            if (st != null)
            {
                for (int j = 0; j < quantity; j++)
                {
                    rollData.Depth++;
                    List<SpawnResult> list2 = _spawnService.Roll(gs, st.Items, rollData);
                    rollData.Depth--;
                    retval.AddRange(list2);
                }
            }
            return retval;
        }
    }
}
