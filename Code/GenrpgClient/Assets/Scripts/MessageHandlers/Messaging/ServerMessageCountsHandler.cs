using Genrpg.Shared.MapServer.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Messaging
{
    public class ServerMessageCountsHandler : BaseClientMapMessageHandler<ServerMessageCounts>
    {
        protected override void InnerProcess(UnityGameState gs, ServerMessageCounts msg, CancellationToken token)
        {
            _dispatcher.Dispatch(gs,msg);
        }
    }
}
