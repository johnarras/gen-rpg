using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class OnStopCastHandler : BaseServerMapMessageHandler<OnStopCast>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnStopCast message)
        {
            obj.AddMessage(message);
        }
    }
}
