using Genrpg.ServerShared.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Players.Constants;
using Genrpg.Shared.Quests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.MapServer.MapMessaging;
using Genrpg.Shared.Quests.Messages;
using Genrpg.Shared.MapServer.Entities;

namespace Genrpg.MapServer.Quests.MessageHandlers
{
    public class OnGetQuestsHandler : BaseServerMapMessageHandler<OnGetQuests>
    {
        protected override void InnerProcess(GameState gs, MapMessagePackage pack, MapObject obj, OnGetQuests message)
        {
            if (!_objectManager.GetChar(obj.Id, out Character ch))
            {
                return;
            }

            ch.AddMessage(message);
        }
    }
}
