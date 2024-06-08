using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Services;
namespace Genrpg.Shared.Zones.Helpers
{
    public class ZoneHelper : IEntityHelper
    {


        protected IMapProvider _mapProvider;
        public long GetKey() { return EntityTypes.Zone; }
        public string GetDataPropertyName() { return "Zones"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _mapProvider.GetMap().Zones.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
