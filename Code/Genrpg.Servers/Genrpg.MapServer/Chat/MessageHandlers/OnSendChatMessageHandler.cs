using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Chat.MessageHandlers
{
    public class OnChatMessageHandler : BaseCharacterServerMapMessageHandler<OnChatMessage>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, OnChatMessage message)
        {
            ch.AddMessage(message);
        }
    }
}
