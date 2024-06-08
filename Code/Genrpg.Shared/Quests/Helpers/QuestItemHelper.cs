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
    public class QuestItemHelper : IEntityHelper
    {
        private IMapProvider _mapProvider = null;

        public long GetKey() { return EntityTypes.QuestItem; }
        public string GetDataPropertyName() { return "QuestItems"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            if (_mapProvider.GetMap() == null ||
                _mapProvider.GetMap().QuestItems == null)
            {
                return null;
            }

            return _mapProvider.GetMap().QuestItems.FirstOrDefault(x => x.IdKey == id);
        }
    }
}
