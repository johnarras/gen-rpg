using ClientEvents;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Loot.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Rewards
{
    public class SendRewardsHandler : BaseClientMapMessageHandler<SendRewards>
    {
        protected IEntityService _entityService;
        protected override void InnerProcess(UnityGameState gs, SendRewards msg, CancellationToken token)
        {
            _entityService.GiveRewards(gs, gs.ch, msg.Rewards);

            if (msg.ShowPopup)
            {
                gs.Dispatch(new ShowLootEvent() { Rewards = msg.Rewards });
            }
        }
    }
}
