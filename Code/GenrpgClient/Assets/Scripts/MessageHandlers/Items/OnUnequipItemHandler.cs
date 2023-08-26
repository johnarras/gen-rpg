using Genrpg.Shared.Inventory.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnUnequipItemHandler : BaseClientMapMessageHandler<OnUnequipItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnUnequipItem msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
