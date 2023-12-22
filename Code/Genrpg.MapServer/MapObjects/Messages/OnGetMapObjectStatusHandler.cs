using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapObjects.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapObjects.Messages
{
    public class OnGetMapObjectStatusHandler : BaseServerMapMessageHandler<OnGetMapObjectStatus>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnGetMapObjectStatus message)
        {
            obj.AddMessage(message);
        }
    }
}
