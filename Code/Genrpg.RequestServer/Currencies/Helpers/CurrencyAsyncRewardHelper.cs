using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.RequestServer.Currencies.Helpers
{
    public class CurrencyAsyncRewardHelper : BaseAsyncOwnerQuantityRewardHelper<CurrencyData, CurrencyStatus>
    {
        public override long GetKey() { return EntityTypes.Currency; }
    }
}
