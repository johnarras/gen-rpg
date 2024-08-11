using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemRewardHelper : IRewardHelper
    {
        public bool GiveReward(IRandom rand, Unit unit, long entityId, long quantity, object extraData = null)
        {
            if (quantity < 1)
            {
                return false;
            }

            return true;
        }

        public long GetKey() { return EntityTypes.QuestItem; }

    }
}
