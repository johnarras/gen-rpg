using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Levels.Messages;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Levelup.MessageHandlers
{
    public class NewLevelHandler : BaseServerMapMessageHandler<NewLevel>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, NewLevel message)
        {
            obj.AddMessage(message);
        }
    }
}
