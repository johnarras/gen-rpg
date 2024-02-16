using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Chat.MessageHandlers
{
    public class OnChatMessageHandler : BaseServerMapMessageHandler<OnChatMessage>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnChatMessage message)
        {
            obj.AddMessage(message);
        }
    }
}
