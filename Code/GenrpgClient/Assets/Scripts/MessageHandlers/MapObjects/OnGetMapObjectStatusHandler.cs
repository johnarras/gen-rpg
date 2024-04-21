using System.Threading;
using Genrpg.Shared.MapObjects.Messages;

namespace Assets.Scripts.MessageHandlers.MapObjects
{
    public class OnGetMapObjectStatusHandler : BaseClientMapMessageHandler<OnGetMapObjectStatus>
    {
        protected override void InnerProcess(UnityGameState gs, OnGetMapObjectStatus msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
