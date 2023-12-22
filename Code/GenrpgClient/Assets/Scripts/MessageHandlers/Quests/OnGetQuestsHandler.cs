using Genrpg.Shared.Quests.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Quests
{
    public class OnGetQuestsHandler : BaseClientMapMessageHandler<OnGetQuests>
    {
        protected override void InnerProcess(UnityGameState gs, OnGetQuests msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
