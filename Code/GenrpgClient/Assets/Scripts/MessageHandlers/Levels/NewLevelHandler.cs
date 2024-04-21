using Genrpg.Shared.Levels.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Levels
{
    public class NewLevelHandler : BaseClientMapMessageHandler<NewLevel>
    {
        protected override void InnerProcess(UnityGameState gs, NewLevel msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
