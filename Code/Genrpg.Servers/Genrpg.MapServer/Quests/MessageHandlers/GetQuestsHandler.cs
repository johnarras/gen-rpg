using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Players.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Quests.MapObjectAddons;
using Genrpg.Shared.MapServer.Entities;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class GetQuestsHandler : BaseServerMapMessageHandler<GetQuests>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, GetQuests message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            if (!_objectManager.GetObject(message.ObjId, out MapObject mobject))
            {
                return;
            }

            QuestAddon addon = mobject.GetAddon<QuestAddon>();

            _messageService.SendMessage(obj, new OnGetQuests() { ObjId = message.ObjId, Quests = addon?.Quests ?? new List<QuestType>() }); ;
        }
    }
}
