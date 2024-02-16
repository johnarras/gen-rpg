
using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Targets
{
    public class OnTargetIsDeadHandler : BaseClientMapMessageHandler<OnTargetIsDead>
    {
        protected override void InnerProcess(UnityGameState gs, OnTargetIsDead msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                unit.AddFlag(UnitFlags.IsDead);
            }
            if (_objectManager.GetController(msg.UnitId, out UnitController controller))
            {
                Died died = new Died()
                {
                    UnitId = msg.UnitId,
                };
                controller.OnDeath(died, token);
            }
        }
    }
}
