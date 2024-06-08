using Genrpg.Shared.Combat.Messages;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.Spawns
{
    public class DiedHandler : BaseClientMapMessageHandler<Died>
    {
        protected override void InnerProcess(Died msg, CancellationToken token)
        {
            if (_objectManager.GetUnit(msg.UnitId, out Unit unit))
            {
                unit.AddFlag(UnitFlags.IsDead);              
                if (msg.FirstAttacker != null)
                {
                    unit.AddAttacker(msg.FirstAttacker.AttackerId, msg.FirstAttacker.GroupId);
                }
            }
            if (_objectManager.GetController(msg.UnitId, out UnitController controller))
            {
                controller.OnDeath(msg, token);
            }
        }
    }
}
