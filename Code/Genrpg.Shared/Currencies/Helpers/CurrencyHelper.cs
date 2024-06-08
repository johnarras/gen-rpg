using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings;

namespace Genrpg.Shared.Currencies.Helpers
{
    public class CurrencyHelper : IEntityHelper
    {

        private IGameData _gameData = null;
        public long GetKey() { return EntityTypes.Currency; }
        public string GetDataPropertyName() { return "CurrencyTypes"; }

        public IIndexedGameItem Find(IFilteredObject obj, long id)
        {
            return _gameData.Get<CurrencySettings>(obj).Get(id);
        }
    }
}
