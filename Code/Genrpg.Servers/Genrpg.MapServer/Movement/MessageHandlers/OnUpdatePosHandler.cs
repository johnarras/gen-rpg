
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Movement.MessageHandlers
{
    public class OnUpdPosHandler : BaseMapObjectServerMapMessageHandler<OnUpdatePos>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnUpdatePos message)
        {
            if (obj.Id != message.ObjId)
            {
                obj.AddMessage(message);
            }
        }
    }
}
