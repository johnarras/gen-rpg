using Genrpg.Shared.Core.Entities;

using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Currencies.Settings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Entities.Helpers;

namespace Genrpg.Shared.Currencies.Helpers
{
    public class CurrencyHelper : BaseEntityHelper<CurrencySettings,CurrencyType>
    {
        public override long GetKey() { return EntityTypes.Currency; }
    }
}
