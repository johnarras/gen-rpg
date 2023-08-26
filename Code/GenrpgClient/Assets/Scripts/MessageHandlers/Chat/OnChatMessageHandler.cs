
using Genrpg.Shared.Chat.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Chat
{
    public class OnChatMessageHandler : BaseClientMapMessageHandler<OnChatMessage>
    {
        protected override void InnerProcess(UnityGameState gs, OnChatMessage msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
