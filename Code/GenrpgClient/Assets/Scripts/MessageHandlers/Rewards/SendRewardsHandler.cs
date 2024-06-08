using ClientEvents;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Loot.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Rewards
{
    public class SendRewardsHandler : BaseClientMapMessageHandler<SendRewards>
    {
        protected IEntityService _entityService;
        protected override void InnerProcess(SendRewards msg, CancellationToken token)
        {
            _entityService.GiveRewards(_rand, _gs.ch, msg.Rewards);

            if (msg.ShowPopup)
            {
                _dispatcher.Dispatch(new ShowLootEvent() { Rewards = msg.Rewards });
            }
        }
    }
}
