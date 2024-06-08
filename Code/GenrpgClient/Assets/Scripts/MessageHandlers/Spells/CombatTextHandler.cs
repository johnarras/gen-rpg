
using Genrpg.Shared.Spells.Messages;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class CombatTextHandler : BaseClientMapMessageHandler<CombatText>
    {
        protected override void InnerProcess(CombatText msg, CancellationToken token)
        {
            if (_objectManager.GetController(msg.TargetId, out UnitController controller))
            {
                controller.ShowCombatText(msg);
            }
        }
    }
}
