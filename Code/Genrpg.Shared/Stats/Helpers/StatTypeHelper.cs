using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Stats.Helpers
{
    public class StatTypeHelper : IEntityHelper
    {
        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.Stat; }
        public string GetDataPropertyName() { return "StatTypes"; }


        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<StatSettings>(obj).Get(id);
        }
    }
}
