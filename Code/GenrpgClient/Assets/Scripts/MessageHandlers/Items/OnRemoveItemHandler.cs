using Genrpg.Shared.Inventory.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnRemoveItemHandler : BaseClientMapMessageHandler<OnRemoveItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnRemoveItem msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
