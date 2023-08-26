using Genrpg.Shared.Inventory.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnAddItemHandler : BaseClientMapMessageHandler<OnAddItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnAddItem msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
