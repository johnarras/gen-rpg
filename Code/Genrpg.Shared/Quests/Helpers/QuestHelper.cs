using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Genrpg.Shared.MapServer.Services;
namespace Genrpg.Shared.Quests.Helpers
{
    public class QuestHelper : IEntityHelper
    {
        private IMapProvider _mapProvider = null;
        public long GetKey() { return EntityTypes.Quest; }
        public string GetDataPropertyName() { return "Quests"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().Quests == null)
            {
                return null;
            }

            return _mapProvider.GetMap().Quests.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
