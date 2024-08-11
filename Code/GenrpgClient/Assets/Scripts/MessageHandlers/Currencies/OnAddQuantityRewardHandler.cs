using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Currencies.PlayerData;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Rewards.Messages;
using Genrpg.Shared.Rewards.Services;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Currency
{
    public class OnAddQuantityRewardHandler : BaseClientMapMessageHandler<OnAddQuantityReward>
    {
        protected IRewardService _rewardService = null;
        protected override void InnerProcess(OnAddQuantityReward msg, CancellationToken token)
        {
            if (msg.CharId != _gs.ch.Id)
            {
                return;
            }
           
            _rewardService.Add(_gs.ch, msg.EntityTypeId, msg.EntityId, msg.Quantity);
            _dispatcher.Dispatch(msg);
        }
    }
}
