using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Utils;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class ResendSpellHandler : BaseMapObjectServerMapMessageHandler<ResendSpell>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, ResendSpell message)
        {
            if (message.ShotsLeft < 1)
            {
                return;
            }            

            if (!_objectManager.GetUnit(message.SpellMessage.CasterId, out Unit caster) ||
                !IsOkUnit(caster,true))
            {
                return;
            }

            if (!_objectManager.GetUnit(message.TargetId, out Unit target) ||
                !IsOkUnit(target, true))
            {
                return;
            }

            _spellService.ResendSpell(rand, caster, target, message.SpellMessage);

            message.ShotsLeft--;

            if (message.ShotsLeft > 0)
            {

                _messageService.SendMessage(caster, message, SpellUtils.GetResendDelay(message.SpellMessage.Spell.HasFlag(SpellFlags.InstantHit)));
            }
        }
    }
}
