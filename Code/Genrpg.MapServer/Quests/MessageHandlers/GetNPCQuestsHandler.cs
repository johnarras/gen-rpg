using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.Entities;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.NPCs.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Quests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Quests.Messages;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class GetNPCQuestsHandler : BaseServerMapMessageHandler<GetNPCQuests>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, GetNPCQuests message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            // TODO make this dynamic.
            List<QuestType> quests = gs.map.GetQuestsForNPC(gs, message.NPCTypeId);

            _messageService.SendMessage(obj, new OnGetNPCQuests() { NPCTypeId = message.NPCTypeId, Quests = quests });
        }
    }
}
