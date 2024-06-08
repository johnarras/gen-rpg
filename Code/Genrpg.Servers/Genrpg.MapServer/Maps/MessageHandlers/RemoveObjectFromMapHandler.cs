using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromMapHandler : BaseMapObjectServerMapMessageHandler<RemoveObjectFromMap>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, RemoveObjectFromMap message)
        {
            _objectManager.RemoveObject(rand, obj.Id);
        }
    }
}
