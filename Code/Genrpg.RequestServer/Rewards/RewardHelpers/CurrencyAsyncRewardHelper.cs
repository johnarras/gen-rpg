using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;

namespace Genrpg.RequestServer.Rewards.RewardHelpers
{
    public class CurrencyAsyncRewardHelper : BaseAsyncOwnerQuantityRewardHelper<CurrencyData, CurrencyStatus>
    {
        public override long GetKey() { return EntityTypes.Currency; }
    }
}
