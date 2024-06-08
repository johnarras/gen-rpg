using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Spells.MessageHandlers
{
    public class FXHandler : BaseMapObjectServerMapMessageHandler<FX>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, MapObject obj, FX message)
        {
            obj.AddMessage(message);
        }
    }
}
