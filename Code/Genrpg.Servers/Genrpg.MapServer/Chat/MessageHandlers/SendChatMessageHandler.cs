using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Chat.Constants;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.MapServer.Chat.MessageHandlers
{
    public class SendChatMessageHandler : BaseServerMapMessageHandler<SendChatMessage>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, SendChatMessage message)
        {
            float radius = 0;
            if (message.ChatTypeId == ChatTypes.Say)
            {
                radius = 20;
            }
            else if (message.ChatTypeId == ChatTypes.Yell)
            {
                radius = 50;
            }

            if (radius > 0)
            {
                List<Character> nearbyChars = _objectManager.GetTypedObjectsNear<Character>(obj.X, obj.Z, null, radius, true);

                OnChatMessage onChatMessage = new OnChatMessage()
                {
                    SenderId = obj.Id,
                    SenderName = obj.Name,
                    ChatTypeId = message.ChatTypeId,
                    Message = message.Text,
                };
                foreach (Character ch in nearbyChars)
                {
                    _messageService.SendMessage(ch, onChatMessage);
                }
            }
        }
    }
}
