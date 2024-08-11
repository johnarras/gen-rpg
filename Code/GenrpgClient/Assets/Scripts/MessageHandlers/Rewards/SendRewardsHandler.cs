using ClientEvents;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Loot.Messages;
using Genrpg.Shared.Rewards.Services;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Rewards
{
    public class SendRewardsHandler : BaseClientMapMessageHandler<SendRewards>
    {
        protected IRewardService _rewardService;
        protected override void InnerProcess(SendRewards msg, CancellationToken token)
        {
            _rewardService.GiveRewards(_rand, _gs.ch, msg.Rewards);

            if (msg.ShowPopup)
            {
                _dispatcher.Dispatch(new ShowLootEvent() { Rewards = msg.Rewards });
            }
        }
    }
}
