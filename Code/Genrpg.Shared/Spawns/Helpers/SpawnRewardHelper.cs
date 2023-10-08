using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnRewardHelper : IRewardHelper
    {
        public long GetKey() { return EntityType.Spawn; }

        public bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null)
        {
            return true;
        }
    }
}
