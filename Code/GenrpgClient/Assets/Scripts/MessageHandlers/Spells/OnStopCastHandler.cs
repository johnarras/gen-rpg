
using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnStopCastHandler : BaseClientMapMessageHandler<OnStopCast>
    {
        protected override void InnerProcess(UnityGameState gs, OnStopCast msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
