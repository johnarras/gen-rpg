using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class AddAttackerHandler : BaseServerMapMessageHandler<AddAttacker>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, AddAttacker message)
        {
            if (!GetOkUnit(obj, false, out Unit unit))
            {
                return;
            }

            unit.AddAttacker(message.AttackerId, null);

        }
    }
}
