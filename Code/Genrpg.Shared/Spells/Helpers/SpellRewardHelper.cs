using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellRewardHelper : IRewardHelper
    {
        public bool GiveReward(GameState gs, Character ch, long entityId, long quantity, object extraData = null)
        {
            return true;
        }
        public long GetKey() { return EntityType.Spell; }
    }
}
