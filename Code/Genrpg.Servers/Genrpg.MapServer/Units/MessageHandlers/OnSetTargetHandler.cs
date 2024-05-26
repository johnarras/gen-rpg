using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Targets.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class OnSetTargetHandler : BaseServerMapMessageHandler<OnSetTarget>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnSetTarget message)
        {
            obj.AddMessage(message);
        }
    }
}
