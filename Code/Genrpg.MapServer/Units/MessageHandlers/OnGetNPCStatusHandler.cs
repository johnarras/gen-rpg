using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.NPCs.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Units.MessageHandlers
{
    public class OnGetNPCStatusHandler : BaseServerMapMessageHandler<OnGetNPCStatus>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnGetNPCStatus message)
        {
            obj.AddMessage(message);
        }
    }
}
