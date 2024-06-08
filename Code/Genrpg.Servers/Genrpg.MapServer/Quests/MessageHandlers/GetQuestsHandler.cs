using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Players.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.Quests.WorldData;
using Genrpg.Shared.Quests.MapObjectAddons;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.MapServer.MapMessaging.MessageHandlers;
using Genrpg.Shared.Utils;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class GetQuestsHandler : BaseCharacterServerMapMessageHandler<GetQuests>
    {
        protected override void InnerProcess(IRandom rand, MapMessagePackage pack, Character ch, GetQuests message)
        {
            if (!_objectManager.GetObject(message.ObjId, out MapObject mobject))
            {
                return;
            }

            QuestAddon addon = mobject.GetAddon<QuestAddon>();

            _messageService.SendMessage(ch, new OnGetQuests() { ObjId = message.ObjId, Quests = addon?.Quests ?? new List<QuestType>() }); ;
        }
    }
}
