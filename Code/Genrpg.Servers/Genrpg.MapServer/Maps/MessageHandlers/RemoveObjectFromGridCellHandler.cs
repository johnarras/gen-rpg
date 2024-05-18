using Genrpg.MapServer.MapMessaging;
using Genrpg.MapServer.Maps.Messaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Maps.MessageHandlers
{
    public class RemoveObjectFromGridCellHandler : BaseServerMapMessageHandler<RemoveObjectFromGridCell>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, RemoveObjectFromGridCell message)
        {
            _objectManager.FinalRemoveObjectFromOldGrid(gs, obj, message.GridData, message.GridItem);
        }
    }
}
