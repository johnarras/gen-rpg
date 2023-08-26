
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
    public class SendSpellHandler : BaseServerMapMessageHandler<SendSpell>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SendSpell message)
        {

            if (!GetOkUnit(obj, true, out Unit unit))
            {
                return;
            }
            _spellService.OnSendSpell(gs, unit, message);
        }
    }
}
