using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.NPCs.Helpers
{
    public class NPCHelper : IEntityHelper
    {
        public long GetKey() { return EntityType.NPC; }
        public string GetDataPropertyName() { return "NPCs"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.map.NPCs.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
