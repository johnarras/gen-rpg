using Genrpg.Shared.SpellCrafting.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnRemoveActionBarItemHandler : BaseClientMapMessageHandler<OnRemoveActionBarItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnRemoveActionBarItem msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
