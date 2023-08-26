
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Spells.Messages;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CastSpellHandler : BaseServerMapMessageHandler<CastSpell>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, CastSpell message)
        {
            if (!GetOkUnit(obj, true, out Unit caster))
            {
                return;
            }

            _spellService.FullTryStartCast(gs, caster, message.SpellId, message.TargetId);

        }
    }
}
