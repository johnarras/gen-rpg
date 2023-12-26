using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Pathfinding.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Pathfinding.MessageHandlers
{
    public class OnMoveToLocationMessageHandler : BaseServerMapMessageHandler<OnMoveToLocation>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnMoveToLocation message)
        {
            obj.AddMessage(message);
        }
    }
}
