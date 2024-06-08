using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapObjects.Messages
{
    public class OnGetMapObjectStatusHandler : BaseMapObjectServerMapMessageHandler<OnGetMapObjectStatus>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, OnGetMapObjectStatus message)
        {
            obj.AddMessage(message);
        }
    }
}
