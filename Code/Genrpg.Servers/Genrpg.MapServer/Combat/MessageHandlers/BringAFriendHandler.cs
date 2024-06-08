using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class BringAFriendHandler : BaseUnitServerMapMessageHandler<BringFriends>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Unit unit, BringFriends message)
        {
            if (unit.IsPlayer() || unit.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
            {
                return;
            }

            if (unit.FactionTypeId != message.BringerFactionId)
            {
                return;
            }

            if (unit.HasTarget())
            {
                unit.AddAttacker(message.TargetId, null);
            }
            else
            {
                _aiService.TargetMove(rand, unit, message.TargetId);
            }
        }
    }
}
