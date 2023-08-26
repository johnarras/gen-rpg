using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Currencies.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Interfaces;
namespace Genrpg.Shared.Currencies.Helpers
{
    public class CurrencyHelper : IEntityHelper
    {

        public long GetKey() { return EntityType.Currency; }
        public string GetDataPropertyName() { return "CurrencyTypes"; }

        public IIndexedGameItem Find(GameState gs, long id)
        {
            return gs.data.GetGameData<CurrencySettings>().GetCurrencyType(id);
        }
    }
}
