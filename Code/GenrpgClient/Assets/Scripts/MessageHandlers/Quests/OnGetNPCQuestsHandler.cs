using Genrpg.Shared.Quests.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Levels
{
    public class OnGetNPCQuestsHandler : BaseClientMapMessageHandler<OnGetNPCQuests>
    {
        protected override void InnerProcess(UnityGameState gs, OnGetNPCQuests msg, CancellationToken token)
        {
            gs.Dispatch(msg);
        }
    }
}
