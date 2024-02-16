using Genrpg.MapServer.AI.Services;
using Genrpg.MapServer.Combat.Messages;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Factories;
using Genrpg.Shared.Units.Constants;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Combat.MessageHandlers
{
    public class BringAFriendHandler : BaseServerMapMessageHandler<BringFriends>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, BringFriends message)
        {
            Unit possibleFriend = obj as Unit;
            if (possibleFriend == null || possibleFriend.IsPlayer() || possibleFriend.HasFlag(UnitFlags.IsDead | UnitFlags.Evading))
            {
                return;
            }

            if (possibleFriend.FactionTypeId != message.BringerFactionId)
            {
                return;
            }

            if (possibleFriend.HasTarget())
            {
                possibleFriend.AddAttacker(message.TargetId, null);
            }
            else
            {
                _aiService.TargetMove(gs, possibleFriend, message.TargetId);
            }
        }
    }
}
