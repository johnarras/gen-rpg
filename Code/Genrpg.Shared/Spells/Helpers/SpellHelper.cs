using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using System.Threading.Tasks;
using Genrpg.Shared.Spells.Settings.Spells;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Spells.Helpers
{

    public class SpellHelper : IEntityHelper
    {
        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.Spell; }
        public string GetDataPropertyName() { return "Spells"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<SpellTypeSettings>(obj).Get(id);
        }
    }
}
