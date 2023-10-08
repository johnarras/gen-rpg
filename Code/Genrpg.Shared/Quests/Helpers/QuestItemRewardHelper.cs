using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemRewardHelper : IRewardHelper
    {
        public bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null)
        {
            if (quantity < 1)
            {
                return false;
            }

            return true;
        }

        public long GetKey() { return EntityType.QuestItem; }

    }
}
