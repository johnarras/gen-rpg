using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnUpdateEffectHandler : BaseClientMapMessageHandler<OnUpdateEffect>
    {
        protected override void InnerProcess(UnityGameState gs, OnUpdateEffect msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
