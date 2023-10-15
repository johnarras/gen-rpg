using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestItemHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.QuestItem; }
        public string GetDataPropertyName() { return "QuestItems"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
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
