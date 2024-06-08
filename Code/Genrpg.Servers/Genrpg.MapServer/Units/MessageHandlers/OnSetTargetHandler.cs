using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Targets.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class OnSetTargetHandler : BaseMapObjectServerMapMessageHandler<OnSetTarget>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnSetTarget message)
        {
            obj.AddMessage(message);
        }
    }
}
