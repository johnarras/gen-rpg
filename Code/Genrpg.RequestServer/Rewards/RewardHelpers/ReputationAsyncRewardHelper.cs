using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.PlayerData;

namespace Genrpg.RequestServer.Rewards.RewardHelpers
{
    public class ReputationAsyncRewardHelper : BaseAsyncOwnerQuantityRewardHelper<ReputationData,ReputationStatus>
    {
        public override long GetKey() { return EntityTypes.Reputation; }
    }
}
