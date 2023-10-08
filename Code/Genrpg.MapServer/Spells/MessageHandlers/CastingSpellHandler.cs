
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Spells.Entities;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Stats.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Targets.Messages;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CastingSpellHandler : BaseServerMapMessageHandler<CastingSpell>
    {
        private IStatService _statService;
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CastingSpell message)
        {
            if (!GetOkUnit(obj, true, out Unit caster))
            {
                obj.ActionMessage = null;
                _spellService.SendStopCast(gs, obj);
                return;
            }

            if (message.Spell == null)
            {
                obj.ActionMessage = null;
                _spellService.SendStopCast(gs, obj);
                pack.SendError(gs, obj, "Spell does not exist");
                return;
            }


            TryCastResult result = _spellService.TryCast(gs, caster, message.Spell.IdKey, message.TargetId, true);

            if (result.State != TryCastState.Ok)
            {
                pack.SendError(gs, obj, result.StateText);
                if (result.State == TryCastState.TargetDead)
                {
                    caster.AddMessage(new OnTargetIsDead() { UnitId = message.TargetId });
                }
                _spellService.SendStopCast(gs, obj);
                return;
            }

            if (caster.ActionMessage != message)
            {
                pack.SendError(gs, obj, "You aren't casting this spell");
                _spellService.SendStopCast(gs, obj);
                return;
            }

            _statService.Add(gs, caster, result.SkillType.PowerStatTypeId, StatCategory.Curr, -result.Spell.GetCost(gs, caster));

            // Send projectile to target.
            _spellService.SendSpell(gs, caster, result);
        }
    }
}
