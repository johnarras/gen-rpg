using Genrpg.Shared.SpellCrafting.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnSetActionBarItemHandler : BaseClientMapMessageHandler<OnSetActionBarItem>
    {
        protected override void InnerProcess(UnityGameState gs, OnSetActionBarItem msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
