using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Utils;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class ResendSpellHandler : BaseServerMapMessageHandler<ResendSpell>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, ResendSpell message)
        {
            if (message.ShotsLeft < 1)
            {
                return;
            }

            if (!_objectManager.GetUnit(message.SpellMessage.CasterId, out Unit caster))
            {
                return;
            }

            if (caster.HasFlag(UnitFlags.IsDead) || caster.IsDeleted())
            {
                return;
            }

            if (!_objectManager.GetUnit(message.TargetId, out Unit target))
            {
                return;
            }

            if (target.HasFlag(UnitFlags.IsDead) || target.IsDeleted())
            {
                return;
            }

            _spellService.ResendSpell(gs, caster, target, message.SpellMessage);

            message.ShotsLeft--;

            if (message.ShotsLeft > 0)
            {

                _messageService.SendMessage(caster, message, SpellUtils.GetResendDelay(message.SpellMessage.Spell.HasFlag(SpellFlags.InstantHit)));
            }
        }
    }
}
