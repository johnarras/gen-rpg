using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Genrpg.Shared.Spawns.Helpers
{
    public class SpawnRewardHelper : IRewardHelper
    {
        public long GetKey() { return EntityTypes.Spawn; }

        public bool GiveReward(IRandom rand, Character ch, long entityId, long quantity, object extraData = null)
        {
            return true;
        }
    }
}
