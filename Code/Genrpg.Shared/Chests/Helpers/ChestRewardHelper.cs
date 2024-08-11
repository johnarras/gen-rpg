using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Helpers;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Chests.Helpers
{
    public class ChestRewardHelper : IRewardHelper
    {
        public long GetKey() { return EntityTypes.Chest; }

        public bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null)
        {
            // Need to get loot from chest and give to player.
            return true;
        }
    }
}
