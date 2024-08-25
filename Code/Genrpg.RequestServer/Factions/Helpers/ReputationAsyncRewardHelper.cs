using Genrpg.RequestServer.Rewards.RewardHelpers.Core;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Factions.PlayerData;

namespace Genrpg.RequestServer.Factions.Helpers
{
    public class ReputationAsyncRewardHelper : BaseAsyncOwnerQuantityRewardHelper<ReputationData, ReputationStatus>
    {
        public override long GetKey() { return EntityTypes.Reputation; }
    }
}
