using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class AddAttackerHandler : BaseUnitServerMapMessageHandler<AddAttacker>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, AddAttacker message)
        {
            if (!IsOkUnit(unit, false))
            {
                return;
            }

            unit.AddAttacker(message.AttackerId, null);

        }
    }
}
