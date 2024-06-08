using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Units.Entities;
using System.Threading.Tasks;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Utils;
namespace Genrpg.Shared.Units.Helpers
{
    public class UnitHelper : IEntityHelper
    {
        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.Unit; }
        public string GetDataPropertyName() { return "UnitTypes"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<UnitSettings>(obj).Get(id);
        }
    }
}
