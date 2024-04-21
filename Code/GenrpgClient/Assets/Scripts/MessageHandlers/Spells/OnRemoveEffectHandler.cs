using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class OnRemoveEffectHandler : BaseClientMapMessageHandler<OnRemoveEffect>
    {
        protected override void InnerProcess(UnityGameState gs, OnRemoveEffect msg, CancellationToken token)
        {
            if (!_objectManager.GetUnit(msg.TargetId, out Unit unit))
            {
                return;
            }

            _dispatcher.Dispatch(gs,msg);
        }
    }
}
