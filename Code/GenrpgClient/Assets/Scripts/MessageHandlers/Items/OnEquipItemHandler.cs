
using Genrpg.Shared.Inventory.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Items
{
    public class OnEquipItemHandler : BaseClientMapMessageHandler<OnEquipItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnEquipItem msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
