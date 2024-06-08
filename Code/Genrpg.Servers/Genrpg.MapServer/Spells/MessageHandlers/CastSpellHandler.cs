
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class CastSpellHandler : BaseUnitServerMapMessageHandler<CastSpell>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, CastSpell message)
        {
            _spellService.FullTryStartCast(rand, unit, message.SpellId, message.TargetId);

        }
    }
}
