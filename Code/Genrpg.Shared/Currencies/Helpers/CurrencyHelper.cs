using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Settings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Currencies.Helpers
{
    public class CurrencyHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.Currency; }
        public string GetDataPropertyName() { return "CurrencyTypes"; }

        public IIndexedGameItem Find(GameState gs, IFilteredObject obj, long id)
        {
            return gs.data.GetGameData<CurrencySettings>(obj).GetCurrencyType(id);
        }
    }
}
