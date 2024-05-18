using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Stats.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Stats.MessageHandlers
{
    public class StatUpdHandler : BaseServerMapMessageHandler<StatUpd>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, StatUpd message)
        {
            obj.AddMessage(message);
        }
    }
}
