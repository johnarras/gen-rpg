using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using System.Threading.Tasks;
namespace Genrpg.Shared.Zones.Helpers
{
    public class ZoneHelper : IEntityHelper
    {
        public long GetKey() { return EntityTypes.Zone; }
        public string GetDataPropertyName() { return "Zones"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.map.Zones.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
