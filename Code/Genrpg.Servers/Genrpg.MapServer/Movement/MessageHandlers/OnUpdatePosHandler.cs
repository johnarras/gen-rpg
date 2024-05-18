
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.MapServer.Entities;

namespace Genrpg.MapServer.Movement.MessageHandlers
{
    public class OnUpdPosHandler : BaseServerMapMessageHandler<OnUpdatePos>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnUpdatePos message)
        {
            if (obj.Id != message.ObjId)
            {
                obj.AddMessage(message);
            }
        }
    }
}
