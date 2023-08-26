using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.QuestItem; }
        public string GetDataPropertyName() { return "QuestItems"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            if (gs.map == null ||
                gs.map.QuestItems == null)
            {
                return null;
            }

            return gs.map.QuestItems.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
