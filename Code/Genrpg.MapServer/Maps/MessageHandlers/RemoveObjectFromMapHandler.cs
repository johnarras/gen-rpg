using Genrpg.MapServer.MapMessaging;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromMapHandler : BaseServerMapMessageHandler<RemoveObjectFromMap>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, RemoveObjectFromMap message)
        {
            _objectManager.RemoveObject(gs, obj.Id);
        }
    }
}
